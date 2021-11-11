using System;

namespace ArtificialLifeSim
{
    class Program
    {
        static void Main(string[] args)
        {
            World world = new World(100, 0.001f, 0.001f, 10, 100);
            world.Run();
        }
    }
}
