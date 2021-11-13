using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialLifeSim
{
    interface IPosition
    {
        Vector2 Position { get; }
        Vector2 Min { get; }
        Vector2 Max { get; }
    }
}
