// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Numerics;

namespace ArtificialLifeSim
{
    class Food : IPosition {
        public float Radius { get; set; }
        public Vector2 Position { get; set; }
        public float Energy = 0.1f;

        public Food(Vector2 position, float energy) {
            Radius = (float)Utils.RandomDouble(0.05f, 1.0f);
            Position = position;
            Energy = 0.9f;//energy;
        }
    }
}