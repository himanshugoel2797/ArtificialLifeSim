using OpenTK.Mathematics;

namespace ArtificialLifeSim.Physics
{
    internal struct Entity : IPosition
    {
        public int[] Nodes { get; set; }
        public int[] Sticks { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }

        public Entity(int[] nodes, int[] sticks)
        {
            Nodes = nodes;
            Sticks = sticks;
            Min = new Vector2(float.MaxValue);
            Max = new Vector2(float.MinValue);
            Position = Vector2.Zero;

            Recenter();
        }

        public void Recenter()
        {
            Min = new Vector2(float.MaxValue);
            Max = new Vector2(float.MinValue);

            ObjectPool.NodePool.AcquireRef();
            try
            {
                for (int i = 0; i < Nodes.Length; i++)
                {
                    ref var node = ref ObjectPool.NodePool.Get(Nodes[i]);
                    Max = Vector2.ComponentMax(Max, node.Position + Vector2.One * node.Radius);
                    Min = Vector2.ComponentMin(Min, node.Position - Vector2.One * node.Radius);
                }
            }
            finally { ObjectPool.NodePool.ReleaseRef(); }
            Position = (Max + Min) * 0.5f;
        }
    }
}
