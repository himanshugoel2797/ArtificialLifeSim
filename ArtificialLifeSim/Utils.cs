// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ArtificialLifeSim
{
    struct Triangle
    {
        public Vector2 A, B, C;
    }

    static class Utils
    {
        static int seed = 0;
        static ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));
        static Utils() { }

        public static double RandomDouble() => random.Value.NextDouble();
        public static bool RandomBool() => random.Value.Next(0, 2) == 0;
        public static bool RandomBool(float mutationChance) => RandomDouble(0, 1) <= mutationChance;
        public static int RandomInt(int min, int max) => random.Value.Next(min, max);
        public static double RandomDouble(double min, double max) => min + (max - min) * random.Value.NextDouble();
        public static Vector2 RandomVector2(double min, double max) => new Vector2((float)RandomDouble(min, max), (float)RandomDouble(min, max));

        public static void ShuffleArray<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = random.Value.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static float Sin(float time, float period, float offset, float start_val, float end_val)
        {
            return (start_val - end_val) * MathF.Abs(MathF.Sin(time * 50 * period + offset * MathF.PI)) + end_val;
        }

        public static int RouletteWheel(IEnumerable<float> set)
        {
            float sel = (float)RandomDouble();
            float sum = 0;
            int idx = 0;
            foreach(var v in set)
            {
                sum += v;
                if (sel <= sum)
                    return idx;
                idx++;
            }
            throw new Exception("Execution shouldn't reach here!");
        }

        public static int[] RouletteWheelMany(IEnumerable<float> set, int n)
        {
            var sel = new int[n];
            for (int i = 0; i < sel.Length; i++) sel[i] = -1;
            for (int i = 0; i < sel.Length; i++)
            {
                var cur_sel = RouletteWheel(set);
                while (sel.Contains(cur_sel)) cur_sel = RouletteWheel(set);
                sel[i] = cur_sel;
            }
            return sel;
        }

        public static Triangle GenerateTriangle(double minX = 0, double minY = 0, double maxX = 1, double maxY = 1)
        {
            double x1 = RandomDouble(minX, maxX);
            double y1 = RandomDouble(minY, maxY);
            double x2 = RandomDouble(minX, maxX);
            double y2 = RandomDouble(minY, maxY);
            double x3 = RandomDouble(minX, maxX);
            double y3 = RandomDouble(minY, maxY);

            return new Triangle
            {
                A = new Vector2((float)x1, (float)y1),
                B = new Vector2((float)x2, (float)y2),
                C = new Vector2((float)x3, (float)y3)
            };
        }

        public static Vector2 PerpendicularClockwise(this Vector2 vector2)
        {
            return new Vector2(vector2.Y, -vector2.X);
        }

        public static Vector2 PerpendicularCounterClockwise(this Vector2 vector2)
        {
            return new Vector2(-vector2.Y, vector2.X);
        }

        public static Triangle[] Triangulate(Vector2[] points)
        {
            var sorted = points.OrderBy(p => p.X).OrderBy(p => p.Y).ToArray();
            var triangles = new List<Triangle>();

            for (int i = 0; i < sorted.Length; i++)
            {
                var p = sorted[i];

                var t = new Triangle
                {
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