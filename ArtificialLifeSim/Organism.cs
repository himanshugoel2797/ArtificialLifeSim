using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtificialLifeSim.Chromosomes;
using ArtificialLifeSim.Physics;
using OpenTK.Mathematics;

namespace ArtificialLifeSim
{
    class Organism
    {
        public World World { get; set; }
        public BodyChromosome BodyChromosome { get; set; }
        public IChromosome[] Genome { get; internal set; }
        public float Energy { get; internal set; }
        public float Age { get; internal set; }
        public float VisionRange { get; internal set; }
        public Entity Body { get; internal set; }

        public Organism(World w)
        {
            World = w;
        }

        public void Setup()
        {

        }

        public void Update(double time)
        {
            foreach (var node in Body.Nodes)
                if (node.Type == BodyNodeType.Mouth | node.Type == BodyNodeType.Eye)
                {
                    //Look for neighboring food
                    var food = World.GetFoodInContext(this, node.Position);
                    for (int i = 0; i < food.Length; i++)
                        lock (food[i])
                            if (food[i].Energy != 0)
                            {
                                Energy += food[0].Energy;
                                food[0].Energy = 0;
                                Console.WriteLine("food consumed");
                                break;
                            }
                }
        }
    }
}
