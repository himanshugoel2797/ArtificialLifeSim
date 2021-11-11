// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System.Linq;

namespace ArtificialLifeSim
{
    class BrainGene : Gene
    {
        public int LayerCount;
        public Layer[] Layers;

        public BrainGene() { }
        public BrainGene(BrainGene other)
        {
            LayerCount = other.LayerCount;
            Layers = new Layer[LayerCount];
            for (int i = 0; i < LayerCount; i++)
            {
                Layers[i] = other.Layers[i].Clone();
            }
        }

        //Inputs: Signals for each eye of distance to nearest other organism, Signals for each muscle/rotator's strength at last tick, Signals for each mouth's distance to food, Current age, Current energy, Current health, clock ticks
        //Outputs: Signals for each muscle's strength

        public float[] Evaluate(float[] inputs)
        {
            var tmp = inputs;
            for (int i = 0; i < LayerCount; i++)
            {
                tmp = Layers[i].Forward(tmp);
            }
            return tmp;
        }

        public void Generate()
        {
            LayerCount = 1;//Utils.RandomInt(1, 5);
            Layers = new Layer[LayerCount];

            int prevInputCnt = OrganismLimits.MaxBrainInputCount;
            for (int i = 0; i < LayerCount - 1; i++)
            {
                int neuronCnt = OrganismLimits.MaxVertexCount;
                Layers[i] = new Layer(neuronCnt, prevInputCnt);
                Layers[i].Randomize();
                prevInputCnt = neuronCnt;
            }
            Layers[LayerCount - 1] = new Layer(OrganismLimits.MaxMuscleCount + OrganismLimits.MaxRotatorCount, prevInputCnt, tanh: true);
            Layers[LayerCount - 1].Randomize();
        }

        public bool IsViable()
        {
            return true;
        }

        public Gene Mate(Genome genome0, Genome genome1, Gene other, MutationOptions options)
        {
            //TODO: For now choose a random brain between the two parents
            if (Utils.RandomBool())
                return new BrainGene(this);
            else
                return new BrainGene(other as BrainGene);
        }

        public Gene Mutate(Genome genome, MutationOptions options)
        {
            BrainGene newGene = new BrainGene(this);
            int layerIdx = Utils.RandomInt(0, LayerCount);
            for (int x = 0; x < newGene.Layers[layerIdx].Neurons.GetLength(0); x++)
                for (int y = 0; y < newGene.Layers[layerIdx].Neurons.GetLength(1); y++)
                    if (options.MutationChance > Utils.RandomDouble())
                        newGene.Layers[layerIdx].Neurons[x, y] *= (float)Utils.RandomDouble(-1, 1);

            return newGene;
        }
    }
}