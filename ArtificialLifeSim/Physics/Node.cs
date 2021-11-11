using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Physics
{
    internal class Node
    {
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public float Radius { get; set; } = 0.1f;
    }

    internal class Stick
    {
        private float length;

        public Node Node0 { get; set; }
        public Node Node1 { get; set; }
        public float Length { get => length; set { length = value; LengthSquared = value * value; } }
        public float LengthSquared { get; private set; }
        public float Stiffness { get; set; }
    }

    internal class Entity
    {
        const int RelaxationStepCount = 1;
        const float CollisionInelasticity = 0.99f;

        public Node[] Nodes { get; set; }
        public Stick[] Sticks { get; set; }

        public Entity(Node[] nodes, Stick[] sticks)
        {
            Nodes = nodes;
            Sticks = sticks;
        }

        public void Update()
        {
            //Update point positions
            foreach (Node node in Nodes)
            {
                var vel = node.Position - node.PreviousPosition;
                node.PreviousPosition = node.Position;
                node.Position += vel;
            }

            for (int i = 0; i < RelaxationStepCount; i++)
            {
                //Update sticks
                foreach (Stick stick in Sticks)
                {
                    var diff = (stick.Node0.Position - stick.Node1.Position);
                    var curStickLenSq = diff.Length;
                    var err = (stick.Length - curStickLenSq) / curStickLenSq * stick.Stiffness;

                    var offset = diff * err * 0.5f;
                    stick.Node0.Position -= offset;
                    stick.Node1.Position += offset;
                }

                //Resolve collisions
                foreach (Node node in Nodes)
                    foreach (Node node1 in Nodes)
                    {
                        if (node1 == node) continue;

                        //Check for circle vs circle collision
                        if ((node1.Position - node.Position).Length <= node1.Radius + node.Radius)
                        {
                            //Resolve the collision
                            var diff0 = (node.Position - node.PreviousPosition) * CollisionInelasticity;
                            var diff1 = (node1.Position - node1.PreviousPosition) * CollisionInelasticity;

                            node.PreviousPosition = node.Position;
                            node.Position -= diff0;

                            node1.PreviousPosition = node1.Position;
                            node1.Position -= diff1;
                        }
                    }
            }
        }
    }
}
