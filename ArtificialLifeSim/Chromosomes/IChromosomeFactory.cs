using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Chromosomes
{
    internal interface IChromosomeFactory<T> where T : IChromosome
    {
        T CreateChromosome();
        T Mate(IChromosome chromosome0, IChromosome chromosome1);
    }
}
