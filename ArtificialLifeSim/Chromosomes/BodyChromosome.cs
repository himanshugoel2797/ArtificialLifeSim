using ArtificialLifeSim.Physics;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Chromosomes
{
    enum BodyNodeType
    {
        Empty = 0,
        Mouth,
        Eye,

        MaxValue,
    }

    enum BodyLinkType
    {
        None = 0,
        Muscle,

        MaxValue,
    }

    class BodyNode
    {
        public BodyNodeType Type;
        public Vector2 Position;
        public bool Active;

        public BodyNode() { }
        public BodyNode(BodyNode bodyNode)
        {
            Type = bodyNode.Type;
            Position = bodyNode.Position;
        }
    }

    class BodyLink
    {
        public BodyLinkType Type;
        public int Node0_Index;
        public int Node1_Index;
        public float Length;
        public float Stiffness;
        public bool Active;
    }

    class BodyChromosome : IChromosome
    {
        public BodyNode[] Nodes { get; set; }
        public BodyLink[] Links { get; set; }

        internal BodyChromosome() { }

        public void Apply(Organism organism)
        {
            //Build an entity object for the organism
            var nodes = new Node[Nodes.Count(a => a.Active)];
            var sticks = new Stick[Links.Count(a => a.Active)];

            int j = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                var node = Nodes[i];
                if (!node.Active) continue;

                nodes[j++] = new Node()
                {
                    Type = node.Type,
                    Position = node.Position,
                    PreviousPosition = node.Position,// + Utils.RandomVector2(-0.01, 0.01),
                    Radius = World.NodeRadius,
                };
            }

            j = 0;
            for (int i = 0; i < Links.Length; i++)
            {
                var link = Links[i];
                if (!link.Active) continue;

                int n0_idx = 0;
                int n1_idx = 0;
                for (int k = 0; k < link.Node0_Index; k++) if (Nodes[k].Active) n0_idx++;
                
                n1_idx += n0_idx;
                for (int k = link.Node0_Index; k < link.Node1_Index; k++) if (Nodes[k].Active) n1_idx++;

                sticks[j++] = new Stick()
                {
                    Node0 = nodes[n0_idx],
                    Node1 = nodes[n1_idx],
                    Length = link.Length,
                    Stiffness = link.Stiffness,
                    Type = link.Type,
                };
            }

            organism.Body = new Entity(nodes, sticks);
        }
    }
}
