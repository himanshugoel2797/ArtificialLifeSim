// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace ArtificialLifeSim
{
    struct Food : IPosition {
        public object Lock { get; set; }
        public float Radius { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }

        public float Energy = 0.1f;

        public Food(Vector2 position, float energy) {
            Radius = 0.5f;
            Position = position;
            Energy = 0.9f;//energy;
            
            var len = MathF.Sqrt(2) * Radius;

            Min = position + Vector2.One * len;
            Max = position - Vector2.One * len;

            Lock = new object();
        }
    }
}