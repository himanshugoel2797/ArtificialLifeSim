using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Chromosomes
{
    internal interface IChromosome
    {
        void Apply(Organism organism);
    }
}
