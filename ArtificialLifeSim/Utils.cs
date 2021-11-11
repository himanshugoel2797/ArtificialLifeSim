// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace ArtificialLifeSim{
    struct Triangle {
        public Vector2 A, B, C;
    }

    class Utils{

        static int seed = 0;
        static ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));
        static Utils(){
        }

        public static double RandomDouble(){
            return random.Value.NextDouble();
        }

        public static bool RandomBool(){
            return random.Value.Next(0, 2) == 0;
        }

        public static int RandomInt(int min, int max){
            return random.Value.Next(min, max);
        }

        public static double RandomDouble(double min, double max){
            return min + (max - min) * random.Value.NextDouble();
        }

        public static Vector2 RandomVector2(double min, double max){
            return new Vector2((float)RandomDouble(min, max), (float)RandomDouble(min, max));
        }

        public static Triangle GenerateTriangle(double minX = 0, double minY = 0, double maxX = 1, double maxY = 1){
            double x1 = RandomDouble(minX, maxX);
            double y1 = RandomDouble(minY, maxY);
            double x2 = RandomDouble(minX, maxX);
            double y2 = RandomDouble(minY, maxY);
            double x3 = RandomDouble(minX, maxX);
            double y3 = RandomDouble(minY, maxY);

            return new Triangle{
                A = new Vector2((float)x1, (float)y1),
                B = new Vector2((float)x2, (float)y2),
                C = new Vector2((float)x3, (float)y3)
            };
        }

        public static Triangle[] Triangulate(Vector2[] points) {
            var sorted = points.OrderBy(p => p.X).OrderBy(p => p.Y).ToArray();
            var triangles = new List<Triangle>();

            for (int i = 0; i < sorted.Length; i++) {
                var p = sorted[i];
                
                var t = new Triangle {
                    A = sorted[i],
                    B = sorted[(i + 1) % sorted.Length],
                    C = sorted[(i + 2) % sorted.Length]
                };

                triangles.Add(t);
            }

            return triangles.ToArray();
        }
    }
}