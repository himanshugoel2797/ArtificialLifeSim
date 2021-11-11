using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim
{
    interface IPosition
    {
        Vector2 Position { get; }
        float Radius { get; }
    }

    class SpatialList<T> where T : IPosition
    {
        List<T>[,] grid;
        int WorldSide = 0;
        int GridSide = 0;

        public int Count { get; set; }

        public SpatialList(int worldSide, int side = 64)
        {
            WorldSide = worldSide;
            GridSide = side;
            grid = new List<T>[side, side];
        }

        public void Add(T item)
        {
            var x_coord = (int)((float)(item.Position.X * GridSide) / WorldSide);
            var y_coord = (int)((float)(item.Position.Y * GridSide) / WorldSide);

            if (grid[x_coord, y_coord] == null) grid[x_coord, y_coord] = new List<T>();
            grid[x_coord, y_coord].Add(item);
            Count++;
        }

        public T[] SearchNeighborhood(T src, Vector2 position, float radSq)
        {
            List<T> results = new List<T>();
            var x_coord = (int)((float)(position.X * GridSide) / WorldSide);
            var y_coord = (int)((float)(position.Y * GridSide) / WorldSide);

            float rad = MathF.Sqrt(radSq);

            for (int x0 = x_coord - 1; x0 < x_coord + 1; x0++)
                for (int y0 = y_coord - 1; y0 < y_coord + 1; y0++)
                {
                    if (x0 < 0 | y0 < 0) continue;
                    if (x0 >= GridSide | y0 >= GridSide) continue;

                    int x = x0;
                    int y = y0;
                    if (grid[x, y] != null)
                        results.AddRange(grid[x, y].Where(x => !x.Equals(src) && (x.Position - position).LengthSquared <= MathF.Pow(rad + x.Radius, 2)));
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

        public void RemoveAll(Predicate<T> func)
        {
            Count = 0;
            foreach (var v in grid)
                if (v != null)
                {
                    v.RemoveAll(func);
                    Count += v.Count;
                }
        }

        public void ParallelForEach(Action<T> task)
        {
            var flattened = grid.Cast<List<T>>().Where(a => a != null).SelectMany(x => x).Cast<T>();
            Parallel.ForEach(flattened, task);
        }

        public void RebuildAll()
        {
            Count = 0;
            for (int x0 = 0; x0 < GridSide; x0++)
                for (int y0 = 0; y0 < GridSide; y0++)
                    if (grid[x0, y0] != null)
                    {
                        var v = grid[x0, y0];
                        for (int i = v.Count - 1; i >= 0; i--)
                        {
                            var item = v[i];

                            var x_coord = (int)((float)(item.Position.X * GridSide) / WorldSide) % GridSide;
                            var y_coord = (int)((float)(item.Position.Y * GridSide) / WorldSide) % GridSide;
                            if (y_coord < 0) y_coord += GridSide;
                            if (x_coord < 0) x_coord += GridSide;

                            if (y_coord != y0 | x_coord != x0)
                            {
                                v.RemoveAt(i);
                                Add(item);
                            }
                        }

                        Count += v.Count;
                    }
        }
    }
}
