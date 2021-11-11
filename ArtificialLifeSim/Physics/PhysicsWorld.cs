using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Physics
{
    internal class PhysicsWorld
    {
        const int RelaxationStepCount = 1;
        const float CollisionInelasticity = 0.99f;
        const float MotionDampingFactor = 0.99f;

        ulong entity_id = 0;
        int worldSide;
        public SpatialGrid<Node> Nodes { get; set; }
        public LinkedList<Stick> Sticks { get; set; }

        public PhysicsWorld(int WorldSide)
        {
            worldSide = WorldSide;
            int gridSide = WorldSide / 10;
            Nodes = new SpatialGrid<Node>(WorldSide, side: gridSide, itemRadius: World.NodeRadius);
            Sticks = new LinkedList<Stick>();
        }

        public void AddEntity(Entity e)
        {
            var ent_id = Interlocked.Increment(ref entity_id);
            foreach (var node in e.Nodes)
                node.EntityID = ent_id;
            foreach (var stick in e.Sticks)
                stick.EntityID = ent_id;

            lock (Nodes) Nodes.AddRange(e.Nodes);
            lock (Sticks)
                foreach (var stick in e.Sticks)
                    Sticks.AddLast(stick);
        }

        public void RemoveEntity(Entity e)
        {
            lock (Nodes)
                foreach (var node in e.Nodes)
                    Nodes.Remove(node);

            lock (Sticks)
            {
                //Stick entries will always be in order so we focus on finding the first node
                var n = Sticks.First;
                var stick_idx = 0;
                while (n != null)
                {
                    var next_n = n.Next;
                    if (n.Value == e.Sticks[stick_idx])
                    {
                        Sticks.Remove(n);
                        stick_idx++;
                    }
                    n = next_n;
                }
            }
        }

        public void Update(float time)
        {
            //Update point positions
            lock (Nodes)
                Nodes.ParallelForEach(node =>
                {
                    node.CurrentFriction = Utils.Sin(time, node.Period, node.TimeOffset, node.StartFriction, node.EndFriction);
                    var vel = (node.Position - node.PreviousPosition) * MotionDampingFactor * node.CurrentFriction;
                    node.PreviousPosition = node.Position;
                    node.Position += vel;
                });

            for (int i = 0; i < RelaxationStepCount; i++)
            {
                //Update sticks
                lock (Sticks)
                    foreach (Stick stick in Sticks)
                    {
                        var diff = (stick.Node0.Position - stick.Node1.Position);
                        var curStickLenSq = diff.Length;
                        stick.Length = stick.StartLength;
                        if (stick.Type == Chromosomes.BodyLinkType.Muscle) stick.Length = Utils.Sin(time, stick.Period, stick.TimeOffset, stick.StartLength, stick.EndLength);
                        var err = (stick.Length - curStickLenSq) / curStickLenSq * stick.Stiffness;

                        var offset = diff * err * 0.5f;

                        var m0 = (stick.Node0.Type == Chromosomes.BodyNodeType.Empty) ? 1.0f : 1.0f;
                        var m1 = (stick.Node1.Type == Chromosomes.BodyNodeType.Empty) ? 1.0f : 1.0f;

                        stick.Node0.Position += offset * m1 / (m0 + m1);
                        stick.Node1.Position -= offset * m0 / (m0 + m1);
                    }

                //Resolve collisions
                lock (Nodes)
                {
                    foreach (Node node in Nodes)
                    {
                        //Check for circle vs circle collisions
                        var neighborhood = Nodes.SearchNeighborhood(node, node.Position, node.Radius);

                        //Neighborhood search should have only returned collisions
                        //Parallel.ForEach(neighborhood, node1 =>
                        foreach (var node1 in neighborhood)
                        //if ((node1.Position - node.Position).Length <= node1.Radius + node.Radius)
                        {
                            if (node1.EntityID == node.EntityID) continue;

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
                    }

                    Nodes.ParallelForEach((node) =>
                    {
                        Vector2 diff0 = node.Position - node.PreviousPosition;
                        if (node.Position.X <= World.NodeRadius | node.Position.X >= worldSide - World.NodeRadius)
                        {
                            node.PreviousPosition = new Vector2(node.Position.X, node.PreviousPosition.Y);
                            node.Position = new Vector2(node.Position.X - diff0.X, node.Position.Y);
                        }
                        if (node.Position.Y <= World.NodeRadius | node.Position.Y >= worldSide - World.NodeRadius)
                        {
                            node.PreviousPosition = new Vector2(node.PreviousPosition.X, node.Position.Y);
                            node.Position = new Vector2(node.Position.X, node.Position.Y - diff0.Y);
                        }
                    });
                }
            }

            Nodes.RebuildAll();
        }
    }
}
