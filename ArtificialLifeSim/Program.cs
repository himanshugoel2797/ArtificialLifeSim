using System;

namespace ArtificialLifeSim
{
    class Program
    {
        static void Main(string[] args)
        {
            World world = new World(1000, 0.01f, 0.01f, 0.1f, 100, 100000);
            world.Run();
        }
    }
}
