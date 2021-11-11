using ArtificialLifeSim.Chromosomes;

namespace ArtificialLifeSim.Physics
{
    internal class Stick
    {
        private float length;

        public Node Node0 { get; set; }
        public Node Node1 { get; set; }
        public float Length { get => length; set { length = value; LengthSquared = value * value; } }
        public float LengthSquared { get; private set; }
        public float Stiffness { get; set; }
        public BodyLinkType Type { get; set; }
    }
}
