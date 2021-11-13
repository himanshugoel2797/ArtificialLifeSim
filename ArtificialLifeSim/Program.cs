using System;

namespace ArtificialLifeSim
{
    class Program
    {
        static void Main(string[] args)
        {
            World world = new World(2000);
            world.Run();
        }
    }
}
