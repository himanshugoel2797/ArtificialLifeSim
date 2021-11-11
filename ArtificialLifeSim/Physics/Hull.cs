// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Numerics;

namespace ArtificialLifeSim.Physics {
    public class Hull {
        public Vector2[] Vertices;
        public Vector2 Position;
        public Vector2 Velocity;
        public float RotationVelocity;
        public float Rotation;
        public float DeltaRotation;
        public float Radius;
        public float RadiusSquared;
        public float Mass;
        public int WorldSide;
        bool freeze = false;

        public Hull(Vector2[] vertices, Vector2 position, float mass, int WorldSide) {
            Vertices = vertices;
            Position = position;
            Mass = mass;

            float maxDistSq = 0;
            foreach (Vector2 v in vertices) {
                float dist = v.LengthSquared();
                if (dist > maxDistSq) {
                    maxDistSq = dist;
                }
            }
            RadiusSquared = maxDistSq;
            Radius = (float)Math.Sqrt(maxDistSq);
            this.WorldSide = WorldSide;
        }

        public void ApplyPush(float amount, Vector2 direction) {
            direction = Vector2.Normalize(direction);
            Velocity += direction * amount / Mass;
            //RotationVelocity += amount / Mass * Vector2.Dot(direction, Vector2.Normalize(Velocity));
        }

        public void ApplyRotation(float amount) {
            RotationVelocity += amount / Mass;
        }

        public void Update(float dt) {
            DeltaRotation = 0;
            if (!freeze)
            {
                Position += Velocity * dt;
                Rotation += DeltaRotation;
                DeltaRotation = RotationVelocity * dt;
                Velocity *= (1 - OrganismLimits.FrictionFactor);
                RotationVelocity *= (1 - OrganismLimits.RotationFrictionFactor);

                if (Position.X < 0) Position.X += WorldSide;
                if (Position.Y < 0) Position.Y += WorldSide;
                Position.X = Position.X % WorldSide;
                Position.Y = Position.Y % WorldSide;
            }
        }

        public static void Collide(Hull o0, Hull o1) {
            //o0.Position = new Vector2(0, -1);
            //o1.Position = new Vector2(0, 1);
            //o0.Velocity = new Vector2(0, 1);
            //o1.Velocity = new Vector2(0, -1);
            //o0.Mass = 0.5f;
            //o1.Mass = 1;

            var v0 = o0.Velocity;
            var v1 = o1.Velocity;
            
            var n = Vector2.Normalize(o1.Position - o0.Position);
            Vector2 rv = v1 - v0;
            
            var v0_m1 = Vector2.Dot((rv), n);
            if (v0_m1 < 0){
                
                var o0_tmpv = (o0.Velocity * (o0.Mass - o1.Mass) + (2 * o1.Mass * o1.Velocity)) / (o0.Mass + o1.Mass);
                var o1_tmpv = (o1.Velocity * (o1.Mass - o0.Mass) + (2 * o0.Mass * o0.Velocity)) / (o0.Mass + o1.Mass);
                o0.Velocity = o0_tmpv;
                o1.Velocity = o1_tmpv;
                //o0.Velocity = -o0.Velocity;
                //o1.Velocity = -o1.Velocity;
                //o0.freeze = true;
                //o1.freeze = true;
            }
        }
    }
}