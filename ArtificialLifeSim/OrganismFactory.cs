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
            bodyChromosome.Apply(organism);
            
            organism.Setup();

            organism.Energy = 1;
            organism.VisionRange = (float)Utils.RandomDouble(0, 10);
            
            return organism;
        }

        public Organism Mate(Organism o0, Organism o1)
        {
            var bodyChromosome = BodyChromosomeFactory.Mate(o0.BodyChromosome, o1.BodyChromosome);
            BodyChromosomeFactory.Mutate(bodyChromosome);

            Organism organism = new Organism(world);
            organism.BodyChromosome = bodyChromosome;
            bodyChromosome.Apply(organism);
            organism.Setup();
            organism.Energy = 1;

            return organism;
        }
    }
}
