// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using OpenTK.Mathematics;
using System;

namespace ArtificialLifeSim.Physics
{
    internal class Hull
    {
        public Organism Parent { get; set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Torque { get; set; }

        public Hull(Organism organism)
        {
            Parent = organism;
        }

        public void ApplyForce(Vector2 force)
        {
            Acceleration += force / Parent.Mass;
        }

        public void ApplyTorque(Vector2 force)
        {
            throw new NotImplementedException();
        }

        public void Update(float dt)
        {
            Velocity += Acceleration * dt;
            Parent.Position += Velocity * dt;

            Acceleration = Vector2.Zero;
        }

        public static void Collide(Hull o0, Hull o1)
        {
            var v0 = o0.Velocity;
            var v1 = o1.Velocity;

            var n = Vector2.Normalize(o1.Parent.Position - o0.Parent.Position);
            Vector2 rv = v1 - v0;

            var v0_m1 = Vector2.Dot((rv), n);
            if (v0_m1 < 0)
            {
                var o0_tmpv = (o0.Velocity * (o0.Parent.Mass - o1.Parent.Mass) + (2 * o1.Parent.Mass * o1.Velocity)) / (o0.Parent.Mass + o1.Parent.Mass);
                var o1_tmpv = (o1.Velocity * (o1.Parent.Mass - o0.Parent.Mass) + (2 * o0.Parent.Mass * o0.Velocity)) / (o0.Parent.Mass + o1.Parent.Mass);
                o0.Velocity = o0_tmpv;
                o1.Velocity = o1_tmpv;
            }
        }
    }
}