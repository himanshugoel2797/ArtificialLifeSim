using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Chromosomes
{
    internal class BodyChromosomeFactory : IChromosomeFactory<BodyChromosome>
    {
        public const int MaxVertexCount = 8;
        public const float MutationChance = 0.05f;
        public const float MutationSize = 0.01f;
        public const float MaxBodySize = 4;

        private float WorldSide;
        public BodyChromosomeFactory(float worldSide)
        {
            WorldSide = worldSide;
        }

        public BodyChromosome CreateChromosome()
        {
            int nodeCount = Utils.RandomInt(2, MaxVertexCount);
            int linkCount = Utils.RandomInt(1, (nodeCount * (nodeCount - 1)) / 2);
            BodyNode[] nodes = new BodyNode[nodeCount];
            BodyLink[] links = new BodyLink[linkCount];

            Vector2 body_pos = Utils.RandomVector2(MaxBodySize * 0.5, WorldSide - MaxBodySize * 0.5);
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = new BodyNode()
                {
                    Position = Utils.RandomVector2(-MaxBodySize * 0.5 - World.NodeRadius, MaxBodySize * 0.5 - World.NodeRadius) + body_pos,
                    Type = (BodyNodeType)Utils.RandomInt(0, (int)BodyNodeType.MaxValue),
                    Active = false,
                };
            for (int i = 0; i < links.Length; i++)
            {
                links[i] = new BodyLink()
                {
                    Node0_Index = Utils.RandomInt(0, nodeCount),
                    Node1_Index = Utils.RandomInt(0, nodeCount),
                    Active = Utils.RandomBool(),
                    Type = (BodyLinkType)Utils.RandomInt(0, (int)BodyLinkType.MaxValue),
                };

                links[i].Stiffness = (links[i].Type == BodyLinkType.Muscle) ? 1.0f : 1.0f;// (float)Utils.RandomDouble(0, 1.0f),
                while (links[i].Node1_Index == links[i].Node0_Index)
                    links[i].Node1_Index = Utils.RandomInt(0, nodeCount);

                //Swap link indices to always be in ascending order
                if (links[i].Node0_Index > links[i].Node1_Index)
                {
                    var tmp = links[i].Node0_Index;
                    links[i].Node0_Index = links[i].Node1_Index;
                    links[i].Node1_Index = tmp;
                }
                links[i].Length = (nodes[links[i].Node0_Index].Position - nodes[links[i].Node1_Index].Position).Length;
            }

            //Turn off duplicate links
            links = links.OrderBy(a => a.Node0_Index).ThenBy(a => a.Node1_Index).ToArray();
            for (int i = 0; i < links.Length - 1; i++)
                if (links[i + 1].Node1_Index == links[i].Node1_Index && links[i + 1].Node0_Index == links[i].Node0_Index)
                    links[i + 1].Active = false;
            if (links.Count(a => a.Active) == 0) links[0].Active = true; //Make sure at least one link is active

            //Activate all nodes reachable from the first active link
            void ActivateNodes(HashSet<int> activated, int node_idx)
            {
                foreach (var link in links)
                {
                    if (!link.Active) continue;
                    if (link.Node0_Index == node_idx && !activated.Contains(link.Node1_Index))
                    {
                        nodes[link.Node1_Index].Active = true;
                        activated.Add(link.Node1_Index);
                        ActivateNodes(activated, link.Node1_Index);
                    }
                    if (link.Node1_Index == node_idx && !activated.Contains(link.Node0_Index))
                    {
                        nodes[link.Node0_Index].Active = true;
                        activated.Add(link.Node0_Index);
                        ActivateNodes(activated, link.Node0_Index);
                    }
                }
            }
            HashSet<int> activated = new HashSet<int>();
            int starting_node = links.First(a => a.Active).Node0_Index;
            nodes[starting_node].Active = true;
            activated.Add(starting_node);
            ActivateNodes(activated, starting_node);

            //Turn off any links that still have any inactive nodes
            foreach (var link in links)
                if (!nodes[link.Node0_Index].Active | !nodes[link.Node1_Index].Active)
                    link.Active = false;

            BodyChromosome bodyChromosome = new BodyChromosome();
            bodyChromosome.Nodes = nodes;
            bodyChromosome.Links = links;
            return bodyChromosome;
        }

        public BodyChromosome Mate(IChromosome chromosome0_, IChromosome chromosome1_)
        {
            var chromosome0 = chromosome0_ as BodyChromosome;
            var chromosome1 = chromosome1_ as BodyChromosome;
            if (chromosome0.Nodes.Length > chromosome1.Nodes.Length)
            {
                var tmp = chromosome0;
                chromosome0 = chromosome1;
                chromosome1 = tmp;
            }

            int crossover_point = Utils.RandomInt(1, chromosome0.Nodes.Length);
            int crossover_len = Utils.RandomInt(crossover_point, chromosome1.Nodes.Length);

            BodyNode[] nodes = new BodyNode[crossover_len];
            for (int i = 0; i < crossover_point; i++)
                nodes[i] = new BodyNode(chromosome0.Nodes[i]);
            for (int i = crossover_point; i < crossover_len; i++)
                nodes[i] = new BodyNode(chromosome1.Nodes[i]);

            BodyChromosome bodyChromosome = new BodyChromosome();
            bodyChromosome.Nodes = nodes;
            return bodyChromosome;
        }

        public void Mutate(IChromosome chromosome_)
        {
            var chromosome = chromosome_ as BodyChromosome;
            for (int i = 0; i < chromosome.Nodes.Length; i++)
            {
                if (Utils.RandomBool(MutationChance))
                    chromosome.Nodes[i].Position += Utils.RandomVector2(-MutationSize, MutationSize);
                if (Utils.RandomBool(MutationChance))
                    chromosome.Nodes[i].Type = (BodyNodeType)Utils.RandomInt(0, (int)BodyNodeType.MaxValue);
            }
        }
    }
}
