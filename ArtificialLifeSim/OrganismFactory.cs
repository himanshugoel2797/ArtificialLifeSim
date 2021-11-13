using ArtificialLifeSim.Chromosomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim
{
    internal class OrganismFactory
    {
        public const int ChromosomeCount = 1;
        World world;

        BodyChromosomeFactory BodyChromosomeFactory { get; set; }

        public OrganismFactory(World w)
        {
            world = w;
            BodyChromosomeFactory = new BodyChromosomeFactory(w.Side);
        }

        public Organism CreateOrganism()
        {
            var bodyChromosome = BodyChromosomeFactory.CreateChromosome();

            Organism organism = new Organism(world);
            organism.BodyChromosome = bodyChromosome;
            RefreshOrganism(organism);
            organism.VisionRange = (float)Utils.RandomDouble(0, 5.0f);
            organism.EatingRange = (float)Utils.RandomDouble(0, 1.0f);

            return organism;
        }

        public Organism Mate(Organism o0, Organism o1)
        {
            var bodyChromosome = BodyChromosomeFactory.Mate(o0.BodyChromosome, o1.BodyChromosome);

            Organism organism = new Organism(world);
            organism.BodyChromosome = bodyChromosome;
            RefreshOrganism(organism);

            float ratio = (float)Utils.RandomDouble();
            organism.VisionRange = (ratio * o0.VisionRange + (1 - ratio) * o1.VisionRange);
            
            ratio = (float)Utils.RandomDouble();
            organism.EatingRange = (ratio * o0.EatingRange + (1 - ratio) * o1.EatingRange);

            return organism;
        }

        public void RefreshOrganism(Organism organism)
        {
            organism.BodyChromosome.Apply(organism);
            organism.Setup();
            organism.Energy = 1;
        }
    }
}
