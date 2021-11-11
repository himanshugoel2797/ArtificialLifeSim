using OpenTK.Mathematics;

namespace ArtificialLifeSim.Features
{
    enum BodyVertexType
    {
        Empty = 0,
        Muscle,
        Mouth,
        Eye,

        MaxValue,
    }

    class BodyNode
    {
        public BodyVertexType VertexType;
        public Vector2 Position;
        public Vector2 RotatedPosition;

        public BodyNode() { }
        public BodyNode(BodyNode bodyNode)
        {
            VertexType = bodyNode.VertexType;
            Position = bodyNode.Position;
        }
    }
}
