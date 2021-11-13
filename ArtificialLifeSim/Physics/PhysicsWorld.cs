using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Physics
{
    internal class PhysicsWorld
    {
        const int RelaxationStepCount = 5;
        const float CollisionInelasticity = 0.99f;
        const float MotionDampingFactor = 0.99f;

        int worldSide;
        int nodeCount;
        int stickCount;

        public SpatialHashMap<Entity> Entities { get; set; }
        public HashSet<int> EntitySet { get; set; }

        public PhysicsWorld(int WorldSide)
        {
            worldSide = WorldSide;
            int gridSide = WorldSide / 10;
            Entities = new SpatialHashMap<Entity>(WorldSide, ObjectPool.EntityPool, 32, 6);
            EntitySet = new HashSet<int>();
        }

        public void Clear()
        {
            Entities.Clear();
            EntitySet.Clear();
            nodeCount = 0;
            stickCount = 0;
        }

        public void AddEntity(int e)
        {
            Entities.Add(e);
            EntitySet.Add(e);
            var ent = ObjectPool.EntityPool[e];
            Interlocked.Add(ref nodeCount, ent.Nodes.Length);
            Interlocked.Add(ref stickCount, ent.Sticks.Length);
        }

        public void RemoveEntity(int e)
        {
            Entities.Remove(e);
            EntitySet.Remove(e);
            var ent = ObjectPool.EntityPool[e];
            Interlocked.Add(ref nodeCount, -ent.Nodes.Length);
            Interlocked.Add(ref stickCount, -ent.Sticks.Length);
        }

        public void Update(float time)
        {
            int[] nodes = new int[nodeCount];
            int[] sticks = new int[stickCount];

            int n_idx = 0;
            int s_idx = 0;
            foreach (var entity in EntitySet)
            {
                var ns = ObjectPool.EntityPool[entity];
                Array.Copy(ns.Nodes, 0, nodes, n_idx, ns.Nodes.Length);
                n_idx += ns.Nodes.Length;

                Array.Copy(ns.Sticks, 0, sticks, s_idx, ns.Sticks.Length);
                s_idx += ns.Sticks.Length;
            }

            //Update point positions
            var node_partition = Partitioner.Create(0, nodes.Length);
            Parallel.ForEach(node_partition, (range, state) =>
            {
                ObjectPool.NodePool.AcquireRef();
                try
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        var node_i = nodes[i];
                        ref var node = ref ObjectPool.NodePool.Get(node_i);
                        if (node.Active)
                        {
                            node.CurrentFriction = Utils.Sin(time, node.Period, node.TimeOffset, node.StartFriction, node.EndFriction);
                            if (node.Type != Chromosomes.BodyNodeType.Empty) node.CurrentFriction = MotionDampingFactor;
                            var vel = (node.Position - node.PreviousPosition) * MotionDampingFactor;
                            node.PreviousPosition = node.Position;
                            node.Position += vel;
                            if (float.IsNaN(node.Position.X) | float.IsNaN(node.Position.Y))
                                throw new Exception();
                        }
                    }
                }
                finally { ObjectPool.NodePool.ReleaseRef(); }
            });

            for (int i = 0; i < RelaxationStepCount; i++)
            {
                //Update sticks
                foreach (var j in sticks)
                {
                    ObjectPool.StickPool.AcquireRef();
                    try
                    {
                        ref var stick = ref ObjectPool.StickPool.Get(j);
                        if (stick.Active)
                        {
                            ObjectPool.NodePool.AcquireRef();
                            try
                            {
                                ref var n0 = ref ObjectPool.NodePool.Get(stick.Node0);
                                ref var n1 = ref ObjectPool.NodePool.Get(stick.Node1);

                                var diff = (n0.Position - n1.Position);
                                if (diff.LengthSquared < 0.00001f)
                                {
                                    //Force one point to be at the target position
                                    diff = Utils.RandomVector2(1, 2).Normalized();
                                    n0.Position += diff;
                                    n0.PreviousPosition += diff;
                                }
                                var curStickLenSq = diff.Length + float.Epsilon;
                                //if (curStickLenSq > 15)
                                //    Console.WriteLine();//throw new Exception();
                                stick.Length = stick.StartLength;
                                if (stick.Type == Chromosomes.BodyLinkType.Muscle) stick.Length = Utils.Sin(time, stick.Period, stick.TimeOffset, stick.StartLength, stick.EndLength);
                                var err = (stick.Length - curStickLenSq) / curStickLenSq;// * (1 + stick.Stiffness);

                                var offset = diff * err;

                                var m0 = n0.CurrentFriction + float.Epsilon; //(stick.Node0.Type == Chromosomes.BodyNodeType.Empty) ? 1.0f : 1.0f;
                                var m1 = n1.CurrentFriction + float.Epsilon; //(stick.Node1.Type == Chromosomes.BodyNodeType.Empty) ? 1.0f : 1.0f;
                                var ratio = m0 / (m0 + m1);
                                if (stick.Type == Chromosomes.BodyLinkType.None) ratio = 0.5f;

                                n0.Position += offset * ratio;
                                n1.Position -= offset * (1 - ratio);

#if DEBUG
                                if ((n0.Position - n1.Position).Length > 15)
                                    throw new Exception();
#endif
                                if (float.IsNaN(n0.Position.X) | float.IsNaN(n0.Position.Y))
                                    throw new Exception();
                                if (float.IsNaN(n1.Position.X) | float.IsNaN(n1.Position.Y))
                                    throw new Exception();
                            }
                            finally { ObjectPool.NodePool.ReleaseRef(); }
                        }
                    }
                    finally { ObjectPool.StickPool.ReleaseRef(); }
                }

                //Resolve collisions
                /*foreach (Node node in Nodes)
                {
                    //Check for circle vs circle collisions
                    var neighborhood = Nodes.SearchNeighborhood(node, node.Position, node.Radius);

                    //Neighborhood search should have only returned collisions
                    //Parallel.ForEach(neighborhood, node1 =>
                    foreach (var node1 in neighborhood)
                    //if ((node1.Position - node.Position).Length <= node1.Radius + node.Radius)
                    {
                        if (node1.EntityID != node.EntityID) continue;

                        //Resolve the collision
                        var diff0 = (node.Position - node.PreviousPosition) * CollisionInelasticity;
                        var diff1 = (node1.Position - node1.PreviousPosition) * CollisionInelasticity;

                        var n = Vector2.Normalize(node1.Position - node.Position);
                        Vector2 rv = diff1 - diff0;

                        if (Vector2.Dot(rv, n) < 0)
                        {
                            node.PreviousPosition = node.Position;
                            node.Position -= diff1;

                            node1.PreviousPosition = node1.Position;
                            node1.Position -= diff0;
                        }
                    }
                }*/

                //TODO: Use grid queries to optimize this, only applies to nodes at the edge of the map
                node_partition = Partitioner.Create(0, nodes.Length);
                Parallel.ForEach(node_partition, (range, state) =>
                {
                    ObjectPool.NodePool.AcquireRef();
                    try
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            var node_i = nodes[i];
                            ref var node = ref ObjectPool.NodePool.Get(node_i);
                            if (node.Active)
                            {
                                Vector2 diff0 = node.Position - node.PreviousPosition;
                                if (node.Position.X <= World.NodeRadius)
                                {
                                    node.Position = new Vector2(World.NodeRadius + float.Epsilon, node.Position.Y);
                                    node.PreviousPosition = new Vector2(node.Position.X + diff0.X, node.PreviousPosition.Y);
                                }
                                else if (node.Position.X >= worldSide - World.NodeRadius)
                                {
                                    node.Position = new Vector2(worldSide - (World.NodeRadius + float.Epsilon), node.Position.Y);
                                    node.PreviousPosition = new Vector2(node.Position.X + diff0.X, node.PreviousPosition.Y);
                                }

                                if (node.Position.Y <= World.NodeRadius)
                                {
                                    node.Position = new Vector2(node.Position.X, World.NodeRadius + float.Epsilon);
                                    node.PreviousPosition = new Vector2(node.PreviousPosition.X, node.Position.Y + diff0.Y);
                                }
                                else if (node.Position.Y >= worldSide - World.NodeRadius)
                                {
                                    node.Position = new Vector2(node.Position.X, worldSide - (World.NodeRadius + float.Epsilon));
                                    node.PreviousPosition = new Vector2(node.PreviousPosition.X, node.Position.Y + diff0.Y);
                                }
#if DEBUG
                                if (float.IsNaN(node.Position.X) | float.IsNaN(node.Position.Y))
                                    throw new Exception();
#endif
                            }
                        }
                    }
                    finally { ObjectPool.NodePool.ReleaseRef(); }
                });
            }

            ObjectPool.EntityPool.AcquireRef();
            try
            {
                foreach (var entity in EntitySet)
                {
                    ref var ns = ref ObjectPool.EntityPool.Get(entity);
                    ns.Recenter();
                }
            }
            finally { ObjectPool.EntityPool.ReleaseRef(); }
            Entities.RebuildAll();
        }
    }
}
