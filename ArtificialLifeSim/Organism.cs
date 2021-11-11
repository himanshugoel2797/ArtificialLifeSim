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
    class Organism : IPosition
    {
        public BodyChromosome BodyChromosome { get; set; }
        public IChromosome[] Genome { get; internal set; }
        public Vector2 Position { get; set; }
        public float Energy { get; internal set; }
        public float Age { get; internal set; }
        public float VisionRange { get; internal set; }
        public Entity Body { get; internal set; }
        
        public Organism()
        {

        }

        public void Setup()
        {

        }

        public void Update(double time)
        {

        }
    }
}
