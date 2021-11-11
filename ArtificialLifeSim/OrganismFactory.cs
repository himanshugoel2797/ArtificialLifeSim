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

        float WorldSide;

        BodyChromosomeFactory BodyChromosomeFactory { get; set; }

        public OrganismFactory(float worldSide)
        {
            BodyChromosomeFactory = new BodyChromosomeFactory();
            WorldSide = worldSide;
        }

        public Organism CreateOrganism()
        {
            IChromosome[] chromosomes = new IChromosome[ChromosomeCount];
            chromosomes[0] = BodyChromosomeFactory.CreateChromosome();

            Organism organism = new Organism();
            organism.Position = Utils.RandomVector2(0, WorldSide);
            organism.Genome = chromosomes;
            foreach (IChromosome chromosome in chromosomes)
                chromosome.Apply(organism);
            organism.Setup();

            organism.Energy = 1;
            organism.VisionRange = (float)Utils.RandomDouble(0, 10);
            organism.Hull.Velocity = Utils.RandomVector2(0, 1);
            
            return organism;
        }

        public Organism Mate(Organism o0, Organism o1)
        {
            IChromosome[] chromosomes = new IChromosome[ChromosomeCount];
            chromosomes[0] = BodyChromosomeFactory.Mate(o0.Genome[0], o1.Genome[0]);
            BodyChromosomeFactory.Mutate(chromosomes[0]);

            Organism organism = new Organism();
            organism.Position = o0.Position + Utils.RandomVector2(o0.Radius, 4 * o0.Radius);
            organism.Genome = chromosomes;
            foreach (IChromosome chromosome in chromosomes)
                chromosome.Apply(organism);
            organism.Setup();
            organism.Energy = 1;

            return organism;
        }
    }
}
