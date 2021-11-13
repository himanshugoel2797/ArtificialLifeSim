using ArtificialLifeSim.Chromosomes;

namespace ArtificialLifeSim.Physics
{
    internal struct Stick
    {
        private float length;

        public bool Active { get; set; }
        public int Node0 { get; set; }
        public int Node1 { get; set; }
        public float Length { get => length; set { length = value; LengthSquared = value * value; } }
        public float LengthSquared { get; private set; }
        public float Stiffness { get; set; }
        public BodyLinkType Type { get; set; }

        public float StartLength { get; set; }
        public float EndLength { get; set; }
        public float Period { get; set; }
        public float TimeOffset { get; set; }
        public ulong EntityID { get; set; }
    }
}
