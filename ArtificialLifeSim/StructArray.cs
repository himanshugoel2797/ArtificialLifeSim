using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtificialLifeSim
{
    internal class StructArray<T> : IDisposable where T : struct
    {
        RWLock _lock;
        RWLock _refLock;

        T[] array;
        Stack<int> freeValues;
        HashSet<int> freed;
        private bool disposedValue;

        public int FarthestEntry { get; private set; }
        public int Count { get; private set; }
        public T[] Data { get => array; }
        public delegate void FreeAction(ref T value);
        public FreeAction OnFree { get; set; } = null;

        public StructArray(int initialLen = 1024)
        {
            _lock = new RWLock();
            _refLock = new RWLock();

            array = new T[initialLen];
            freed = new HashSet<int>();
            freeValues = new Stack<int>();
            Clear();
        }

        private int _allocate()
        {
            //Double array length and copy everything over
            if (freeValues.Count == 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    freeValues.Push(i + array.Length);
                    freed.Add(i + array.Length);
                }

                _refLock.EnterWriteLock();
                try
                {
                    var arr_larger = new T[array.Length * 2];
                    Array.Copy(array, arr_larger, array.Length);
                    array = arr_larger;
                    Console.WriteLine("Growing StructArray");
                }
                finally { _refLock.ExitWriteLock(); }
            }
            Count++;
            var res = freeValues.Pop();
            if (res > FarthestEntry)
                FarthestEntry = res;
            freed.Remove(res);
            return res;
        }

        public int Allocate()
        {
            _lock.EnterWriteLock();
            try
            {
                return _allocate();
            }
            finally { _lock.ExitWriteLock(); }
        }

        public int Allocate(T val)
        {
            _lock.EnterWriteLock();
            try
            {
                int v = _allocate();
                array[v] = val;
                return v;
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void Free(int idx)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!freed.Contains(idx))
                {
                    if (OnFree != null) OnFree(ref array[idx]);
                    freeValues.Push(idx);
                    freed.Add(idx);
                    Count--;
                    if (idx == FarthestEntry)
                        FarthestEntry--;

                }
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                Count = 0;
                FarthestEntry = 0;
                freeValues.Clear();
                freed.Clear();
                for (int i = 0; i < array.Length; i++)
                {
                    freeValues.Push(i);
                    freed.Add(i);
                }
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void AcquireRef()
        {
            _lock.EnterReadLock();
            _refLock.EnterReadLock();
        }

        public ref T Get(int idx)
        {
            return ref array[idx];
        }

        public void ReleaseRef()
        {
            _refLock.ExitReadLock();
            _lock.ExitReadLock();
        }

        public T this[int idx]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return array[idx];
                }
                finally { _lock.ExitReadLock(); }
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    array[idx] = value;
                }
                finally { _lock.ExitWriteLock(); }
            }
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                array = null;
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~StructArray()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
