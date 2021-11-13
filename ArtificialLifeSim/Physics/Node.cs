using ArtificialLifeSim.Chromosomes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim.Physics
{
    internal struct Node
    {
        public bool Active { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public float Radius { get; set; } = 0.1f;
        public BodyNodeType Type { get; set; }
        public float StartFriction { get; set; }
        public float EndFriction { get; set; }
        public float CurrentFriction { get; set; }
        public float Period { get; set; }
        public float TimeOffset { get; set; }
        public ulong EntityID { get; set; }
    }
}
