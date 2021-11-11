using System;

namespace ArtificialLifeSim
{
    class Program
    {
        static void Main(string[] args)
        {
            World world = new World(100, 100, 1000);
            world.Run();
        }
    }
}
