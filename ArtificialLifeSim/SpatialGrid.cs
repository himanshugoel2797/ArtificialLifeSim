using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtificialLifeSim
{
    internal class SpatialGrid<T> where T : struct, IPosition
    {
        LinkedList<int>[,] Grid;
        StructArray<T> DataSrc;
        int WorldSide = 0;
        int GridSide = 0;
        float nodeRad = 0;

        private int count;
        public int Count { get => count; set => count = value; }

        public SpatialGrid(int worldSide, StructArray<T> dataSrc, int side = 64, float itemRadius = 0.1f)
        {
            WorldSide = worldSide;
            GridSide = side;
            nodeRad = itemRadius;
            DataSrc = dataSrc;
            Grid = new LinkedList<int>[side, side];
        }

        public void Clear()
        {
            Count = 0;
            Grid = new LinkedList<int>[GridSide, GridSide];
        }

        public void Add(int item)
        {
            var v = DataSrc[item];
            var x_coord = (int)((float)(v.Position.X * GridSide) / WorldSide);
            var y_coord = (int)((float)(v.Position.Y * GridSide) / WorldSide);

            if (Grid[x_coord, y_coord] == null) Grid[x_coord, y_coord] = new LinkedList<int>();
            Grid[x_coord, y_coord].AddLast(item);
            Count++;
        }

        public void AddRange(params int[] items)
        {
            foreach (var item in items) Add(item);
        }

        public int[] SearchNeighborhood(int src, Vector2 position, float rad)
        {
            List<int> results = new List<int>();
            var x_coord = (int)((float)(position.X * GridSide) / WorldSide);
            var y_coord = (int)((float)(position.Y * GridSide) / WorldSide);

            float netRad = MathF.Pow(rad + nodeRad, 2);

            for (int x0 = x_coord - 1; x0 < x_coord + 1; x0++)
                for (int y0 = y_coord - 1; y0 < y_coord + 1; y0++)
                {
                    if (x0 < 0 | y0 < 0 | x0 >= GridSide | y0 >= GridSide) continue;
                    if (Grid[x0, y0] != null)
                    {
                        var n = Grid[x0, y0].First;
                        while(n != null)
                        {
                            var n_next = n.Next;
                            if (!n.Value.Equals(src) && (DataSrc[n.Value].Position - position).LengthSquared <= netRad)
                                results.Add(n.Value);
                            n = n_next;
                        }
                    }
                }

            return results.OrderBy(x => (DataSrc[x].Position - position).LengthSquared).ToArray();
        }

        public IEnumerator<int> GetEnumerator()
        {
            foreach (var v in Grid)
                if (v != null)
                    foreach (var t in v)
                        yield return t;
        }

        public void Remove(int item)
        {
            var v = DataSrc[item];
            var x_coord = (int)((float)(v.Position.X * GridSide) / WorldSide);
            var y_coord = (int)((float)(v.Position.Y * GridSide) / WorldSide);

            if (Grid[x_coord, y_coord] == null) return;
            Grid[x_coord, y_coord].Remove(item);
            Count--;
        }

        public void RemoveAll(Predicate<int> func)
        {
            Count = 0;
            var flattened = Grid.Cast<LinkedList<int>>().Where(a => a != null);
            //Parallel.ForEach(flattened, v =>
            foreach(var v in flattened)
            {
                var n = v.First;
                while (n != null)
                {
                    var next_n = n.Next;
                    if (func(n.Value))
                        v.Remove(n);
                    n = next_n;
                }
                Interlocked.Add(ref count, v.Count);
            }//);
        }

        public void ParallelForEach(Action<int> task)
        {
            var flattened = Grid.Cast<LinkedList<int>>().Where(a => a != null).SelectMany(x => x).Cast<int>();
            Parallel.ForEach(flattened, task);
        }

        public void RebuildAll()
        {
            for (int x0 = 0; x0 < GridSide; x0++)
                for (int y0 = 0; y0 < GridSide; y0++)
                    if (Grid[x0, y0] != null)
                    {
                        var v = Grid[x0, y0];
                        var n = v.First;
                        while (n != null)
                        {
                            var next_n = n.Next;
                            var item = DataSrc[n.Value];
                            var x_coord = (int)((float)(item.Position.X * GridSide) / WorldSide);
                            var y_coord = (int)((float)(item.Position.Y * GridSide) / WorldSide);

                            if (y_coord != y0 | x_coord != x0)
                            {
                                Add(n.Value);
                                v.Remove(n);
                            }

                            n = next_n;
                        }
                    }

            Count = 0;
            foreach (var v in Grid)
                if (v != null)
                    Count += v.Count;
        }
    }
}
