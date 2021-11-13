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
        public float StartFriction;
        public float EndFriction;
        public float Period;
        public float TimeOffset;

        public BodyNode() { }
        public BodyNode(BodyNode a)
        {
            Type = a.Type;
            Position = a.Position;
            Active = false;
            StartFriction = a.StartFriction;
            EndFriction = a.EndFriction;
            Period = a.Period;
            TimeOffset = a.TimeOffset;
        }
    }

    class BodyLink
    {
        public BodyLinkType Type;
        public int Node0_Index;
        public int Node1_Index;
        public float Stiffness;
        public float StartLength;
        public float EndLength;
        public float Period;
        public float TimeOffset;
        public bool Active;

        public BodyLink() { }
        public BodyLink(BodyLink a)
        {
            Type = a.Type;
            Node0_Index = a.Node0_Index;
            Node1_Index = a.Node1_Index;
            Stiffness = a.Stiffness;
            StartLength = a.StartLength;
            EndLength = a.EndLength;
            Period = a.Period;
            TimeOffset = a.TimeOffset;
            Active = a.Active;
        }
    }

    class BodyChromosome : IChromosome
    {
        public BodyNode[] Nodes { get; set; }
        public BodyLink[] Links { get; set; }
        public Vector2 CenterPosition { get; set; }

        internal BodyChromosome() { }

        public void Apply(Organism organism)
        {
            //Build an entity object for the organism
            var nodes = new int[Nodes.Count(a => a.Active)];
            var sticks = new int[Links.Count(a => a.Active)];

            int j = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                var node = Nodes[i];
                if (!node.Active) continue;

                nodes[j++] = ObjectPool.NodePool.Allocate(new Node()
                {
                    Type = node.Type,
                    Position = node.Position,
                    PreviousPosition = node.Position,// + Utils.RandomVector2(-0.01, 0.01),
                    Radius = World.NodeRadius,
                    StartFriction = node.StartFriction,
                    EndFriction = node.EndFriction,
                    TimeOffset = node.TimeOffset,
                    Period = node.Period,
                    Active = true,
                });
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

                if (n0_idx == n1_idx)
                    throw new Exception();

                sticks[j++] = ObjectPool.StickPool.Allocate(new Stick()
                {
                    Node0 = nodes[n0_idx],
                    Node1 = nodes[n1_idx],
                    StartLength = link.StartLength,
                    EndLength = link.EndLength,
                    Period = link.Period,
                    TimeOffset = link.TimeOffset,
                    Stiffness = link.Stiffness,
                    Type = link.Type,
                    Active = true,
                });
            }

            organism.Body = ObjectPool.EntityPool.Allocate(new Entity(nodes, sticks));
        }
    }
}
