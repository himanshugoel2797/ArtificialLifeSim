using ArtificialLifeSim.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Chromosomes
{
    class BodyChromosome : IChromosome
    {
        public BodyNode[] BodyNodes { get; set; }

        internal BodyChromosome() { }

        public void Apply(Organism organism)
        {
            organism.Body = new Body() { Nodes = BodyNodes };

            float maxDistSq = 0;
            foreach (var n in organism.Body.Nodes)
            {
                float dist = n.Position.LengthSquared;
                if (dist > maxDistSq)
                    maxDistSq = dist;
            }
            organism.RadiusSquared = maxDistSq;
            organism.Radius = MathF.Sqrt(maxDistSq);
            organism.Mass = MathF.PI * maxDistSq;
        }
    }
}
