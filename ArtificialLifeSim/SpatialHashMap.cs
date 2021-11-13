using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtificialLifeSim
{
    class RWLock
    {
        // if lock is above this value then somebody has a write lock
        const int _writerLock = 1000000;
        // lock state counter
        int _lock;
        ReaderWriterLock _lockObj = new ReaderWriterLock();

        public void EnterReadLock() => _lockObj.AcquireReaderLock(-1);
        public void ExitReadLock() => _lockObj.ReleaseReaderLock();

        public void EnterWriteLock() => _lockObj.AcquireWriterLock(-1);
        public void ExitWriteLock() => _lockObj.ReleaseWriterLock();

        /*
        public void EnterReadLock()
        {
            var w = new SpinWait();
            var tmpLock = _lock;
            while (tmpLock >= _writerLock ||
                tmpLock != Interlocked.CompareExchange(ref _lock, tmpLock + 1, tmpLock))
            {
                w.SpinOnce();
                tmpLock = _lock;
            }
        }

        public void EnterWriteLock()
        {
            var w = new SpinWait();

            while (0 != Interlocked.CompareExchange(ref _lock, _writerLock, 0))
            {
                w.SpinOnce();
            }
        }

        public void ExitReadLock()
        {
            Interlocked.Decrement(ref _lock);
        }

        public void ExitWriteLock()
        {
            _lock = 0;
        }*/
    }

    internal class SpatialHashMap<T> where T : struct, IPosition
    {

        LinkedList<int>[][][] _grid;
        HashSet<int> _recorded_vals;
        RWLock _lock;


        public StructArray<T> DataSource { get; set; }
        public int Count { get; private set; }
        public int WorldSide { get; set; }
        public int[] GridSides { get; set; }
        public int[] GridCounts { get; set; }
        public int[] GridCountsX { get; set; }
        public int[][] GridCountsY { get; set; }
        public int TopGridEntrySide { get; set; }
        public int GridLevelCount;

        public SpatialHashMap(int worldSide, StructArray<T> dataSrc, int topGridSide = 1, int gridLevels = 1)
        {
            DataSource = dataSrc;
            WorldSide = worldSide;
            GridLevelCount = gridLevels;

            _grid = new LinkedList<int>[GridLevelCount][][];
            _recorded_vals = new HashSet<int>();
            _lock = new RWLock();
            GridSides = new int[GridLevelCount];
            GridCounts = new int[GridLevelCount];

            GridCountsX = new int[GridLevelCount];
            GridCountsY = new int[GridLevelCount][];

            TopGridEntrySide = topGridSide;
            for (int i = 0; i < GridLevelCount; i++)
            {
                GridSides[i] = (int)MathF.Ceiling(worldSide / (float)(topGridSide >> i));
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _grid.Length; i++)
            {
                _grid[i] = null;
                GridCounts = new int[GridLevelCount];

                GridCountsX = new int[GridLevelCount];
                GridCountsY = new int[GridLevelCount][];

            }
            Count = 0;
        }

        private void Hash(Vector2 pos, int lv, out int x, out int y)
        {
            Hash(pos.X, pos.Y, lv, out x, out y);
        }

        private void Hash(float pos_x, float pos_y, int lv, out int x, out int y)
        {
            x = (int)((float)(pos_x / (TopGridEntrySide >> lv)));
            y = (int)((float)(pos_y / (TopGridEntrySide >> lv)));
        }

        private int TargetGridLevel(Vector2 min, Vector2 max)
        {
            var diff = max - min;
            int maxSide = (int)MathF.Ceiling(MathF.Max(diff.X, diff.Y));

            while (TopGridEntrySide < maxSide)
            {
                if (TopGridEntrySide * 2 > WorldSide)
                    throw new Exception("Object too large for world!");

                //Add a new top level grid
                TopGridEntrySide = TopGridEntrySide * 2;
                GridLevelCount++;
                var n_gridSides = new int[GridLevelCount];
                var n_gridCounts = new int[GridLevelCount];
                var n_gridCountsX = new int[GridLevelCount];
                var n_gridCountsY = new int[GridLevelCount][];
                var n_grid = new LinkedList<int>[GridLevelCount][][];

                n_gridSides[0] = (int)MathF.Ceiling(WorldSide / (float)TopGridEntrySide);
                n_gridCounts[0] = 0;
                Array.Copy(GridSides, 0, n_gridSides, 1, GridLevelCount - 1);
                Array.Copy(GridCounts, 0, n_gridCounts, 1, GridLevelCount - 1);
                Array.Copy(GridCountsX, 0, n_gridCountsX, 1, GridLevelCount - 1);
                Array.Copy(GridCountsY, 0, n_gridCountsY, 1, GridLevelCount - 1);
                Array.Copy(_grid, 0, n_grid, 1, GridLevelCount - 1);

                GridSides = n_gridSides;
                GridCounts = n_gridCounts;
                GridCountsX = n_gridCountsX;
                GridCountsY = n_gridCountsY;
                _grid = n_grid;

                Console.WriteLine($"Grid Level Count Increased! {GridLevelCount}");
            }
            for (int i = 0; i < GridLevelCount - 1; i++)
                if ((TopGridEntrySide >> i) >= maxSide && maxSide > (TopGridEntrySide >> (i + 1)))
                    return i;
            return GridLevelCount - 1;
        }

        #region Add
        private void _add(int item)
        {
            var v = DataSource[item];
            var lv = TargetGridLevel(v.Min, v.Max);
            Hash(v.Position, lv, out int x_hash, out int y_hash);

            if (_grid[lv] == null)
                _grid[lv] = new LinkedList<int>[GridSides[lv]][];
            if (_grid[lv][x_hash] == null)
            {
                _grid[lv][x_hash] = new LinkedList<int>[GridSides[lv]];
                GridCountsX[lv]++;
            }
            if (_grid[lv][x_hash][y_hash] == null)
            {
                LinkedList<int> ent = new LinkedList<int>();
                ent.AddLast(item);
                _grid[lv][x_hash][y_hash] = ent;

                if (GridCountsY[lv] == null) GridCountsY[lv] = new int[GridSides[lv]];
                GridCountsY[lv][x_hash]++;
            }
            else
                _grid[lv][x_hash][y_hash].AddLast(item);

            _recorded_vals.Add(item);
            Count++;
            GridCounts[lv]++;
        }
        public void Add(int item)
        {
            _lock.EnterWriteLock(); ;
            try
            {
                _add(item);
            }
            finally
            {
                _lock.ExitWriteLock(); ;
            }
        }
        #endregion

        public void AddRange(params int[] items)
        {
            foreach (var item in items) Add(item);
        }

        public int[] SearchNeighborhood(Vector2 position, float search_radius)
        {
            var search_radius_sq = search_radius * search_radius;
            List<int> results = new List<int>();

            _lock.EnterReadLock();
            try
            {
                DataSource.AcquireRef();
                try
                {
                    for (int lv = 0; lv < GridLevelCount; lv++)
                    {
                        if (_grid[lv] == null) continue;

                        int start_x = (int)Math.Clamp(MathF.Floor(position.X - search_radius), 0, GridSides[lv] - 1);
                        int stop_x = (int)Math.Clamp(MathF.Ceiling(position.X + search_radius), 0, GridSides[lv] - 1);
                        int start_y = (int)Math.Clamp(MathF.Floor(position.Y - search_radius), 0, GridSides[lv] - 1);
                        int stop_y = (int)Math.Clamp(MathF.Ceiling(position.Y + search_radius), 0, GridSides[lv] - 1);

                        Hash(start_x, start_y, lv, out start_x, out start_y);
                        Hash(stop_x, stop_y, lv, out stop_x, out stop_y);

                        var _grid_v = _grid[lv];
                        for (int i = start_x; i <= stop_x; i++)
                            for (int j = start_y; j <= stop_y; j++)
                            {
                                if (_grid_v[i] != null && _grid_v[i][j] != null)
                                {
                                    var n = _grid_v[i][j].First;
                                    while (n != null)
                                    {
                                        if ((DataSource.Get(n.Value).Position - position).LengthSquared <= search_radius_sq)
                                            results.Add(n.Value);
                                        n = n.Next;
                                    }
                                }
                            }
                    }

                    return results.OrderBy(a => (DataSource.Get(a).Position - position).LengthSquared).ToArray();
                }
                finally { DataSource.ReleaseRef(); }
            }
            finally
            {
                _lock.ExitReadLock(); ;
            }

        }

        public IEnumerator<int> GetEnumerator()
        {
            _lock.EnterReadLock(); ;
            try
            {
                /*foreach (var grid in _grid)
                    if (grid != null)
                        foreach (var a in grid)
                            if (a != null)
                                foreach (var b in a)
                                    if (b != null)
                                        foreach (var v in b)
                                            yield return v;*/
                foreach (var item in _recorded_vals)
                    yield return item;
            }
            finally
            {
                _lock.ExitReadLock(); ;
            }
        }

        public void Remove(int item)
        {
            _lock.EnterWriteLock(); ;
            try
            {
                var v = DataSource[item];
                var lv = TargetGridLevel(v.Min, v.Max);
                Hash(v.Position, lv, out var x_hash, out var y_hash);

                if (_grid[lv] == null)
                    return;
                if (_grid[lv][x_hash] == null)
                    return;
                if (_grid[lv][x_hash][y_hash] == null)
                    return;

                var _ls = _grid[lv][x_hash][y_hash];
                if (_ls.Count == 1)
                {
                    _grid[lv][x_hash][y_hash] = null;
                    GridCountsY[lv][x_hash]--;
                    if (GridCountsY[lv][x_hash] == 0)
                    {
                        _grid[lv][x_hash] = null;
                        GridCountsX[lv]--;
                        if (GridCountsX[lv] == 0)
                        {
                            _grid[lv] = null;
                            GridCountsY[lv] = null;
                        }
                    }
                }
                else
                    _ls.Remove(item);
                _recorded_vals.Remove(item);
                Count--;
                GridCounts[lv]--;
            }
            finally
            {
                _lock.ExitWriteLock(); ;
            }
        }

        public void RemoveAll(Predicate<int> func)
        {
            _lock.EnterWriteLock(); ;
            try
            {
                Count = 0;
                for (int i = 0; i < GridLevelCount; i++)
                {
                    GridCounts[i] = 0;
                    var _g = _grid[i];
                    if (_g != null)
                        for (int i0 = 0; i0 < _g.Length; i0++)
                        {
                            var _x = _g[i0];
                            if (_x != null)
                                for (int i1 = 0; i1 < _x.Length; i1++)
                                {
                                    LinkedList<int> v = _x[i1];
                                    if (v != null)
                                    {
                                        var n = v.First;
                                        while (n != null)
                                        {
                                            var next_n = n.Next;
                                            if (func(n.Value))
                                            {
                                                _recorded_vals.Remove(n.Value);
                                                v.Remove(n);
                                            }
                                            n = next_n;
                                        }
                                        if (v.Count == 0)
                                        {
                                            _x[i1] = null;
                                            GridCountsY[i][i0]--;
                                            if (GridCountsY[i][i0] == 0)
                                            {
                                                _grid[i][i0] = null;
                                                GridCountsX[i]--;
                                                if (GridCountsX[i] == 0)
                                                {
                                                    _grid[i] = null;
                                                    GridCountsY[i] = null;
                                                }
                                            }
                                        }
                                        Count += v.Count;
                                        GridCounts[i] += v.Count;
                                    }
                                }
                        }
                }
            }
            finally
            {
                _lock.ExitWriteLock(); ;
            }
        }

        public void FreeAll(Predicate<int> func)
        {
            _lock.EnterWriteLock(); ;
            try
            {
                Count = 0;
                for (int i = 0; i < GridLevelCount; i++)
                {
                    GridCounts[i] = 0;
                    var _g = _grid[i];
                    if (_g != null)
                    {
                        var partitioner = Partitioner.Create(0, _grid[i].Length);
                        Parallel.ForEach(partitioner, (range, _) =>
                        //for (int i0 = 0; i0 < _g.Length; i0++)
                        {
                            for (int i0 = range.Item1; i0 < range.Item2; i0++)
                            {
                                var _x = _g[i0];
                                if (_x != null)
                                    for (int i1 = 0; i1 < _x.Length; i1++)
                                    {
                                        LinkedList<int> v = _x[i1];
                                        if (v != null)
                                        {
                                            var n = v.First;
                                            while (n != null)
                                            {
                                                var next_n = n.Next;
                                                if (func(n.Value))
                                                {
                                                    DataSource.Free(n.Value);
                                                    _recorded_vals.Remove(n.Value);
                                                    v.Remove(n);
                                                }
                                                n = next_n;
                                            }
                                            if (v.Count == 0)
                                            {
                                                _x[i1] = null;
                                                GridCountsY[i][i0]--;
                                                if (GridCountsY[i][i0] == 0)
                                                {
                                                    _grid[i][i0] = null;
                                                    GridCountsX[i]--;
                                                    if (GridCountsX[i] == 0)
                                                    {
                                                        _grid[i] = null;
                                                        GridCountsY[i] = null;
                                                    }
                                                }
                                            }
                                            Count += v.Count;
                                            GridCounts[i] += v.Count;
                                        }
                                    }
                            }
                        });
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock(); ;
            }
        }

        public void RebuildAll()
        {
            _lock.EnterWriteLock(); ;
            try
            {
                for (int i = 0; i < GridLevelCount; i++)
                {
                    var _g = _grid[i];
                    if (_g != null)
                        for (int _xi = 0; _xi < _g.Length; _xi++)
                        {
                            var _x = _g[_xi];
                            if (_x != null)
                                for (int _yi = 0; _yi < _x.Length; _yi++)
                                {
                                    var v = _x[_yi];
                                    if (v != null)
                                    {
                                        var n = v.First;
                                        while (n != null)
                                        {
                                            var next_n = n.Next;
                                            var item = DataSource[n.Value];
                                            Hash(item.Position, i, out var x_hash, out var y_hash);

                                            var curLvlCnt = GridLevelCount; //Check if level count has changed
                                            var tgt_lvl = TargetGridLevel(item.Min, item.Max);
                                            i += (GridLevelCount - curLvlCnt); //Increase i by the number of levels added to the grid to maintain a reference to the correct level

                                            if (tgt_lvl != i | x_hash != _xi | y_hash != _yi)
                                            {
                                                _add(n.Value);

                                                _recorded_vals.Remove(n.Value);
                                                v.Remove(n);
                                            }

                                            n = next_n;
                                        }
                                        if (v.Count == 0)
                                        {
                                            _x[_yi] = null;
                                            GridCountsY[i][_xi]--;
                                            if (GridCountsY[i][_xi] == 0)
                                            {
                                                _grid[i][_xi] = null;
                                                GridCountsX[i]--;
                                                if (GridCountsX[i] == 0)
                                                {
                                                    _grid[i] = null;
                                                    GridCountsY[i] = null;
                                                }
                                            }
                                        }
                                    }
                                }
                        }
                }
            }
            finally
            {
                _lock.ExitWriteLock(); ;
            }
        }
    }
}
