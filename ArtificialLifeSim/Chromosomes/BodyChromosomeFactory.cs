using ArtificialLifeSim.Features;
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
        public const int MaxVertexCount = 32;
        public const float MutationChance = 0.05f;
        public const float MutationSize = 0.01f;

        private void RecenterNodes(BodyNode[] nodes)
        {
            Vector2 center = Vector2.Zero;
            foreach (BodyNode node in nodes)
                center += node.Position;
            center /= nodes.Length;

            foreach (BodyNode node in nodes)
                node.Position -= center;
        }

        public BodyChromosome CreateChromosome()
        {
            int nodeCount = Utils.RandomInt(2, MaxVertexCount);
            BodyNode[] nodes = new BodyNode[nodeCount];
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = new BodyNode()
                {
                    Position = Utils.RandomVector2(-0.5, 0.5),
                    VertexType = (BodyVertexType)Utils.RandomInt(0, (int)BodyVertexType.MaxValue)
                };
            RecenterNodes(nodes);

            BodyChromosome bodyChromosome = new BodyChromosome();
            bodyChromosome.BodyNodes = nodes;
            return bodyChromosome;
        }

        public BodyChromosome Mate(IChromosome chromosome0_, IChromosome chromosome1_)
        {
            var chromosome0 = chromosome0_ as BodyChromosome;
            var chromosome1 = chromosome1_ as BodyChromosome;
            if (chromosome0.BodyNodes.Length > chromosome1.BodyNodes.Length)
            {
                var tmp = chromosome0;
                chromosome0 = chromosome1;
                chromosome1 = tmp;
            }

            int crossover_point = Utils.RandomInt(1, chromosome0.BodyNodes.Length);
            int crossover_len = Utils.RandomInt(crossover_point, chromosome1.BodyNodes.Length);

            BodyNode[] nodes = new BodyNode[crossover_len];
            for (int i = 0; i < crossover_point; i++)
                nodes[i] = new BodyNode(chromosome0.BodyNodes[i]);
            for (int i = crossover_point; i < crossover_len; i++)
                nodes[i] = new BodyNode(chromosome1.BodyNodes[i]);

            BodyChromosome bodyChromosome = new BodyChromosome();
            bodyChromosome.BodyNodes = nodes;
            return bodyChromosome;
        }

        public void Mutate(IChromosome chromosome_)
        {
            var chromosome = chromosome_ as BodyChromosome;
            for (int i = 0; i < chromosome.BodyNodes.Length; i++)
            {
                if (Utils.RandomBool(MutationChance))
                    chromosome.BodyNodes[i].Position += Utils.RandomVector2(-MutationSize, MutationSize);
                if (Utils.RandomBool(MutationChance))
                    chromosome.BodyNodes[i].VertexType = (BodyVertexType)Utils.RandomInt(0, (int)BodyVertexType.MaxValue);
            }
        }
    }
}
