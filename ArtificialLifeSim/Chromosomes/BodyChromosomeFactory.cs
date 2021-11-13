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
        public const int MaxLinkCount = (MaxVertexCount * (MaxVertexCount - 1)) / 2;
        public const float MutationChance = 0.05f;
        public const float MutationSize = 0.01f;
        public const float MaxBodySize = 10;
        public const float MaxLinkLen = 1.41421356f * MaxBodySize;
        public const float BodyRange = 0.55f;

        private float WorldSide;
        public BodyChromosomeFactory(float worldSide)
        {
            WorldSide = worldSide;
        }

        private void Fixup(BodyNode[] nodes, BodyLink[] links)
        {
#if DEBUG
            if (nodes.Count(a => a.Active) > 0) 
                throw new Exception();
#endif

            //Turn off invalid links
            foreach (var link in links)
                if (link.Node0_Index >= nodes.Length | link.Node1_Index >= nodes.Length)
                    link.Active = false;

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
                        if (link.Node1_Index >= nodes.Length)
                            link.Active = false;
                        else
                        {
                            nodes[link.Node1_Index].Active = true;
                            activated.Add(link.Node1_Index);
                            ActivateNodes(activated, link.Node1_Index);
                        }
                    }
                    if (link.Node1_Index == node_idx && !activated.Contains(link.Node0_Index))
                    {
                        if (link.Node0_Index >= nodes.Length)
                            link.Active = false;
                        else
                        {
                            nodes[link.Node0_Index].Active = true;
                            activated.Add(link.Node0_Index);
                            ActivateNodes(activated, link.Node0_Index);
                        }
                    }
                }
            }
            HashSet<int> activated = new HashSet<int>();
            int starting_node = links.First(a => a.Active).Node0_Index;
            nodes[starting_node].Active = true;
            activated.Add(starting_node);
            ActivateNodes(activated, starting_node);

            //Turn off any links that still have any inactive nodes
            HashSet<int> reachableNodes = new HashSet<int>(nodes.Length);
            foreach (var link in links)
            {
                if (!link.Active) continue;
                if (!nodes[link.Node0_Index].Active | !nodes[link.Node1_Index].Active)
                    link.Active = false;
                if (link.Active)
                {
                    reachableNodes.Add(link.Node0_Index);
                    reachableNodes.Add(link.Node1_Index);
                }
            }

            //Turn off any nodes not reachable by links
            for (int i = 0; i < nodes.Length; i++)
                if (!reachableNodes.Contains(i) && nodes[i].Active)
                    nodes[i].Active = false;

#if DEBUG
            if (nodes.Count(a => a.Active) > links.Count(a => a.Active) + 1)
                throw new Exception();
#endif
        }

        public BodyChromosome CreateChromosome()
        {
            int nodeCount = Utils.RandomInt(2, MaxVertexCount);
            int linkCount = Utils.RandomInt(1, (nodeCount * (nodeCount - 1)) / 2);
            BodyNode[] nodes = new BodyNode[nodeCount];
            BodyLink[] links = new BodyLink[linkCount];

            Vector2 body_pos = Utils.RandomVector2(MaxBodySize * 0.5 + WorldSide * (1 - BodyRange), WorldSide * BodyRange - MaxBodySize * 0.5);
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = new BodyNode()
                {
                    Position = Utils.RandomVector2(-MaxBodySize * 0.5 - World.NodeRadius, MaxBodySize * 0.5 - World.NodeRadius) + body_pos,
                    Type = (BodyNodeType)Utils.RandomInt(0, (int)BodyNodeType.MaxValue),
                    Active = false,
                    StartFriction = (float)Utils.RandomDouble(0, 1),
                    EndFriction = (float)Utils.RandomDouble(0, 1),
                    Period = (float)Utils.RandomDouble(0, 1),
                    TimeOffset = (float)Utils.RandomDouble(0, 1),
                };
            for (int i = 0; i < links.Length; i++)
            {
                links[i] = new BodyLink()
                {
                    Node0_Index = Utils.RandomInt(0, nodeCount),
                    Node1_Index = Utils.RandomInt(0, nodeCount),
                    Active = Utils.RandomBool(),
                    Type = (BodyLinkType)Utils.RandomInt(0, (int)BodyLinkType.MaxValue),
                    Period = (float)Utils.RandomDouble(0, 1),
                    TimeOffset = (float)Utils.RandomDouble(0, 1),
                };

                links[i].Stiffness = (float)Utils.RandomDouble(0.9f, 1.0f);
                while (links[i].Node1_Index == links[i].Node0_Index)
                    links[i].Node1_Index = Utils.RandomInt(0, nodeCount);

                //Swap link indices to always be in ascending order
                if (links[i].Node0_Index > links[i].Node1_Index)
                {
                    var tmp = links[i].Node0_Index;
                    links[i].Node0_Index = links[i].Node1_Index;
                    links[i].Node1_Index = tmp;
                }
                links[i].StartLength = Math.Max(2 * World.NodeRadius + float.Epsilon, (nodes[links[i].Node0_Index].Position - nodes[links[i].Node1_Index].Position).Length);
                links[i].EndLength = Math.Clamp(links[i].StartLength + (float)Utils.RandomDouble(0, links[i].StartLength), 2 * World.NodeRadius + float.Epsilon, MaxLinkLen);
            }

            Fixup(nodes, links);

            BodyChromosome bodyChromosome = new BodyChromosome();
            bodyChromosome.Nodes = nodes;
            bodyChromosome.Links = links;
            bodyChromosome.CenterPosition = body_pos;
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
            int crossover_len = Utils.RandomInt(crossover_point + 1, chromosome1.Nodes.Length);

            Vector2 body_pos = Utils.RandomVector2(MaxBodySize * 0.5 + WorldSide * (1 - BodyRange), WorldSide * BodyRange - MaxBodySize * 0.5);
            BodyNode[] nodes = new BodyNode[crossover_len];
            for (int i = 0; i < crossover_point; i++)
            {
                nodes[i] = new BodyNode(chromosome0.Nodes[i]);
                nodes[i].Position = (nodes[i].Position - chromosome0.CenterPosition) + body_pos;
            }
            for (int i = crossover_point; i < crossover_len; i++)
            {
                nodes[i] = new BodyNode(chromosome1.Nodes[i]);
                nodes[i].Position = (nodes[i].Position - chromosome1.CenterPosition) + body_pos;
            }

            BodyLink[] links = chromosome0.Links.Concat(chromosome1.Links).Select(a => new BodyLink(a)).ToArray();
            //Fixup(nodes, links);

            //Get rid of enough links to stay within MaxLinkCount
            if (links.Length > MaxLinkCount)
            {
                var link_list = new LinkedList<BodyLink>(links);

                while (link_list.Count > MaxLinkCount)
                {
                    var iter = link_list.First;
                    while (iter != null)
                    {
                        var iter_n = iter.Next;
                        if (!iter.Value.Active && Utils.RandomBool())
                            link_list.Remove(iter);
                        iter = iter_n;
                    }
                }
                links = link_list.ToArray();
            }

            BodyChromosome bodyChromosome = new BodyChromosome();
            bodyChromosome.Nodes = nodes;
            bodyChromosome.Links = links;
            bodyChromosome.CenterPosition = body_pos;
            Mutate(bodyChromosome);
            return bodyChromosome;
        }

        private void Mutate(IChromosome chromosome_)
        {
            var chromosome = chromosome_ as BodyChromosome;
            var nodes = chromosome.Nodes;
            var links = chromosome.Links;
            foreach (BodyNode v in nodes)
            {
                if (Utils.RandomBool(MutationChance))
                    v.Position = chromosome.CenterPosition + Vector2.Clamp(v.Position - chromosome.CenterPosition + Utils.RandomVector2(-MutationSize, MutationSize), new Vector2(-MaxBodySize * 0.5f), new Vector2(MaxBodySize * 0.5f));
                if (Utils.RandomBool(MutationChance))
                    v.Type = (BodyNodeType)Utils.RandomInt(0, (int)BodyNodeType.MaxValue);
                if (Utils.RandomBool(MutationChance))
                    v.StartFriction = Math.Clamp(v.StartFriction + (float)Utils.RandomDouble(-MutationSize, MutationSize), 0, 1);
                if (Utils.RandomBool(MutationChance))
                    v.EndFriction = Math.Clamp(v.EndFriction + (float)Utils.RandomDouble(-MutationSize, MutationSize), 0, 1);
                if (Utils.RandomBool(MutationChance))
                    v.Period = Math.Clamp(v.Period + (float)Utils.RandomDouble(-MutationSize, MutationSize), 0, 1);
                if (Utils.RandomBool(MutationChance))
                    v.TimeOffset = Math.Clamp(v.TimeOffset + (float)Utils.RandomDouble(-MutationSize, MutationSize), 0, 1);
            }

            foreach (BodyLink v in links)
            {
                if (Utils.RandomBool(MutationChance))
                    v.Type = (BodyLinkType)Utils.RandomInt(0, (int)BodyLinkType.MaxValue);
                if (Utils.RandomBool(MutationChance))
                    v.StartLength = Math.Clamp(v.StartLength + (float)Utils.RandomDouble(-MutationSize, MutationSize), 0, MaxLinkLen);
                if (Utils.RandomBool(MutationChance))
                    v.EndLength = Math.Clamp(v.EndLength + (float)Utils.RandomDouble(-MutationSize, MutationSize), 0, MaxLinkLen);
                if (Utils.RandomBool(MutationChance))
                    v.Period = Math.Clamp(v.Period + (float)Utils.RandomDouble(-MutationSize, MutationSize), 0, 1);
                if (Utils.RandomBool(MutationChance))
                    v.TimeOffset = Math.Clamp(v.TimeOffset + (float)Utils.RandomDouble(-MutationSize, MutationSize), 0, 1);
                v.Active ^= Utils.RandomBool(MutationChance);
            }

            Fixup(nodes, links);
        }
    }
}
