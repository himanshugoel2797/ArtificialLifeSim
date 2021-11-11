using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtificialLifeSim
{
    internal class SpatialGrid<T> where T : IPosition
    {
        LinkedList<T>[,] grid;
        int WorldSide = 0;
        int GridSide = 0;
        float nodeRad = 0;
        private int count;

        public int Count { get => count; set => count = value; }

        public SpatialGrid(int worldSide, int side = 64, float itemRadius = 0.1f)
        {
            WorldSide = worldSide;
            GridSide = side;
            nodeRad = itemRadius;
            grid = new LinkedList<T>[side, side];
        }

        public void Add(T item)
        {
            var x_coord = (int)((float)(item.Position.X * GridSide) / WorldSide);
            var y_coord = (int)((float)(item.Position.Y * GridSide) / WorldSide);

            if (grid[x_coord, y_coord] == null) grid[x_coord, y_coord] = new LinkedList<T>();
            grid[x_coord, y_coord].AddLast(item);
            Count++;
        }

        public void AddRange(params T[] items)
        {
            foreach (var item in items) Add(item);
        }

        public T[] SearchNeighborhood(T src, Vector2 position, float rad)
        {
            List<T> results = new List<T>();
            var x_coord = (int)((float)(position.X * GridSide) / WorldSide);
            var y_coord = (int)((float)(position.Y * GridSide) / WorldSide);

            float netRad = MathF.Pow(rad + nodeRad, 2);

            for (int x0 = x_coord - 1; x0 < x_coord + 1; x0++)
                for (int y0 = y_coord - 1; y0 < y_coord + 1; y0++)
                {
                    if (x0 < 0 | y0 < 0 | x0 >= GridSide | y0 >= GridSide) continue;
                    if (grid[x0, y0] != null)
                        results.AddRange(grid[x0, y0].Where(x => !x.Equals(src) && (x.Position - position).LengthSquared <= netRad));
                }

            return results.OrderBy(x => (x.Position - position).LengthSquared).ToArray();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var v in grid)
                if (v != null)
                    foreach (var t in v)
                        yield return t;
        }

        public void Remove(T item)
        {
            var x_coord = (int)((float)(item.Position.X * GridSide) / WorldSide);
            var y_coord = (int)((float)(item.Position.Y * GridSide) / WorldSide);

            if (grid[x_coord, y_coord] == null) return;
            grid[x_coord, y_coord].Remove(item);
            Count--;
        }

        public void RemoveAll(Predicate<T> func)
        {
            Count = 0;
            var flattened = grid.Cast<LinkedList<T>>().Where(a => a != null);
            Parallel.ForEach(flattened, v =>
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
            });
        }

        public void ParallelForEach(Action<T> task)
        {
            var flattened = grid.Cast<LinkedList<T>>().Where(a => a != null).SelectMany(x => x).Cast<T>();
            Parallel.ForEach(flattened, task);
        }

        public void RebuildAll()
        {
            for (int x0 = 0; x0 < GridSide; x0++)
                for (int y0 = 0; y0 < GridSide; y0++)
                    if (grid[x0, y0] != null)
                    {
                        var v = grid[x0, y0];
                        var n = v.First;
                        while (n != null)
                        {
                            var next_n = n.Next;
                            var x_coord = (int)((float)(n.Value.Position.X * GridSide) / WorldSide);
                            var y_coord = (int)((float)(n.Value.Position.Y * GridSide) / WorldSide);

                            if (y_coord != y0 | x_coord != x0)
                            {
                                Add(n.Value);
                                v.Remove(n);
                            }

                            n = next_n;
                        }
                    }

            Count = 0;
            foreach (var v in grid)
                if (v != null)
                    Count += v.Count;
        }
    }
}
