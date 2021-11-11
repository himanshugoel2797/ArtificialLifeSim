// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Linq;

namespace ArtificialLifeSim{
    class Layer {
        public float[,] Neurons;
        public float[] Biases;
        public bool Tanh;

        private float[] outputs;

        public Layer(int numNeurons, int numInputs, bool tanh = false) {
            Neurons = new float[numNeurons, numInputs];
            Biases = new float[numNeurons];
            Tanh = tanh;
            outputs = new float[numNeurons];
        }

        public Layer(float[,] neurons, float[] biases, bool tanh = false) {
            Neurons = neurons;
            Biases = biases;
            Tanh = tanh;
            outputs = new float[Neurons.GetLength(0)];
        }

        public Layer Clone() {
            return new Layer((float[,])Neurons.Clone(), (float[])Biases.Clone(), Tanh);
        }

        public void Randomize(float scale = 1.0f) {
            for (int i = 0; i < Neurons.GetLength(0); i++) {
                for (int j = 0; j < Neurons.GetLength(1); j++) {
                    Neurons[i, j] += scale * (float)Utils.RandomDouble(-1, 1);
                }
                Biases[i] += scale * (float)Utils.RandomDouble(-1, 1);
            }
        }

        public float[,] GetWeights() {
            return Neurons;
        }

        public float[] GetBiases() {
            return Biases;
        }

        public float[] Forward(float[] activations) {
            for (int i = 0; i < Neurons.GetLength(0); i++) {
                float sum = 0;
                for (int j = 0; j < Neurons.GetLength(1); j++) {
                    sum += Neurons[i, j] * activations[j];
                }
                //sum += Biases[i];
                if (Tanh)
                    outputs[i] = (float)Math.Tanh(sum);
                else
                    outputs[i] = (sum > 0) ? sum : 0.2f * sum;
            }
            return outputs;
        }
    }
}