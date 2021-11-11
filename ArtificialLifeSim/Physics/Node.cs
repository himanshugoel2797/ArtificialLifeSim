using ArtificialLifeSim.Chromosomes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Physics
{
    internal class Node : IPosition
    {
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public float Radius { get; set; } = 0.1f;
        public BodyNodeType Type { get; set; }
    }
}
