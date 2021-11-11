using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim
{
    class MutationOptions
    {
        public double MutationChance { get; set; } = 0.2;
        public double MutationSize { get; set; } = 0.1;
        public double CrossoverChance { get; internal set; } = 0.1;
    }

    interface Gene
    {
        void Generate();
        Gene Mutate(Genome genome, MutationOptions options);
        Gene Mate(Genome genome0, Genome genome1, Gene other, MutationOptions options);
        bool IsViable();
    }

    class Genome
    {
        private Gene[] Genes;

        public Genome(params Gene[] genes)
        {
            Genes = genes;
        }

        public Genome Mutate(MutationOptions options)
        {
            var n_genes = new Gene[Genes.Length];
            for (int i = 0; i < Genes.Length; i++)
                n_genes[i] = Genes[i].Mutate(this, options);
            return new Genome(n_genes);
        }

        public Genome Mate(Genome other, MutationOptions options)
        {
            var n_genes = new Gene[Genes.Length];
            for (int i = 0; i < Genes.Length; i++)
                n_genes[i] = Genes[i].Mate(this, other, other.Genes[i], options);
            return new Genome(n_genes);
        }
    }
}
