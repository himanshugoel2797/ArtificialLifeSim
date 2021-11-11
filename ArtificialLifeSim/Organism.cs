using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtificialLifeSim.Chromosomes;
using ArtificialLifeSim.Features;
using ArtificialLifeSim.Physics;
using OpenTK.Mathematics;

namespace ArtificialLifeSim
{
    class Organism : IPosition
    {
        public IChromosome[] Genome { get; internal set; }
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public float Radius { get; internal set; }
        public float RadiusSquared { get; internal set; }
        public float Mass { get; internal set; }
        public float Energy { get; internal set; }
        public float Age { get; internal set; }
        public float VisionRange { get; internal set; }
        public Body Body { get; internal set; }
        public Hull Hull { get; internal set; }
        
        public Organism()
        {

        }

        private void RotateBody()
        {
            var rot = Matrix2.CreateRotation(MathF.Acos(Vector2.Dot(Direction, Vector2.UnitX)));
            for (int i = 0; i < Body.Nodes.Length; i++)
                Body.Nodes[i].RotatedPosition = Body.Nodes[i].Position * rot;
        }

        public void Setup()
        {
            Hull = new Hull(this);
        }

        public void Update(double time)
        {
            RotateBody();
        }

        public void UpdatePhysics(double time, double dt)
        {
            Hull.Update((float)dt);
        }
    }
}
