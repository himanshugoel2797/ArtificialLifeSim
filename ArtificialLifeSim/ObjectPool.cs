using ArtificialLifeSim.Chromosomes;
using ArtificialLifeSim.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim
{
    internal static class ObjectPool
    {
        public static StructArray<Node> NodePool { get; set; }
        public static StructArray<Stick> StickPool { get; set; }
        public static StructArray<Entity> EntityPool { get; set; }
        public static StructArray<Food> FoodPool { get; set; }

        static ObjectPool()
        {
            NodePool = new StructArray<Node>()
            {
                OnFree = (ref Node x) => x.Active = false
            };
            StickPool = new StructArray<Stick>()
            {
                OnFree = (ref Stick x) => x.Active = false
            };
            EntityPool = new StructArray<Entity>()
            {
                OnFree = (ref Entity x) =>
                {
                    foreach (var n in x.Nodes)
                        NodePool.Free(n);
                    foreach (var s in x.Sticks)
                        StickPool.Free(s);
                }
            };
            FoodPool = new StructArray<Food>();
        }
    }
}
