namespace ArtificialLifeSim.Physics
{
    internal class Entity
    {
        const int RelaxationStepCount = 1;
        const float CollisionInelasticity = 0.99f;

        public Node[] Nodes { get; set; }
        public Stick[] Sticks { get; set; }

        public Entity(Node[] nodes, Stick[] sticks)
        {
            Nodes = nodes;
            Sticks = sticks;
        }
    }
}
