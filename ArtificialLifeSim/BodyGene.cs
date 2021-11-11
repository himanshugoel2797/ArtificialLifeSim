// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace ArtificialLifeSim
{

    enum BodyVertexType
    {
        None = 0,
        Muscle,
        Mouth,
        Eye,
        Rotator,

        MaxValue,
    }

    class BodyGene : Gene
    {
        public List<Vector2> Vertices { get; set; }
        public List<BodyVertexType> VertexTypes { get; set; }

        private float Area = 0;

        public BodyGene()
        {
            Vertices = new List<Vector2>();
            VertexTypes = new List<BodyVertexType>();
        }

        public BodyGene(BodyGene other)
        {
            Vertices = new List<Vector2>(other.Vertices);
            VertexTypes = new List<BodyVertexType>(other.VertexTypes);
        }

        public float Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public void Rotate(float angle)
        {
            Vector2 ApplyRotation(Vector2 p, OpenTK.Mathematics.Matrix2 rot)
            {
                var tmp = (new OpenTK.Mathematics.Vector2(p.X, p.Y) * rot);
                return new Vector2(tmp.X, tmp.Y);
            }

            var rot = OpenTK.Mathematics.Matrix2.CreateRotation(angle);
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = ApplyRotation(Vertices[i], rot);
            }
        }

        public bool IsConvex()
        {
            int n = Vertices.Count;
            if (n < 3) return false;

            bool isConvex = true;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                int k = (i + 2) % n;

                Vector2 a = Vertices[i];
                Vector2 b = Vertices[j];
                Vector2 c = Vertices[k];

                Vector2 ab = b - a;
                Vector2 ac = c - a;

                float cross = Cross(ab, ac);
                if (cross < 0)
                {
                    isConvex = false;
                    break;
                }
            }

            return isConvex;
        }

        public float CalculateArea()
        {
            int n = Vertices.Count;
            if (n < 3) return 0;

            float max_rad = 0.0f;
            for (int i = 0; i < n; i++)
            {
                var dist = Vertices[i].LengthSquared();
                if (dist > max_rad)
                    max_rad = dist;
            }

            Area = (float)(Math.PI * max_rad);
            return Area;
        }

        public bool IsViable()
        {
            return Vertices.Count >= 3
                   && CalculateArea() > 0
                   && VertexTypes.Where(x => x == BodyVertexType.Eye).Count() <= OrganismLimits.MaxEyeCount
                   && VertexTypes.Where(x => x == BodyVertexType.Mouth).Count() <= OrganismLimits.MaxMouthCount
                   && VertexTypes.Where(x => x == BodyVertexType.Muscle).Count() <= OrganismLimits.MaxMuscleCount
                   && VertexTypes.Where(x => x == BodyVertexType.Rotator).Count() <= OrganismLimits.MaxRotatorCount
                   && VertexTypes.Where(x => x == BodyVertexType.Muscle).Count() > 0;
        }

        public void Generate()
        {
            int n = Utils.RandomInt(3, OrganismLimits.MaxVertexCount + 10);
            while (!IsViable())
            {
                Vertices.Clear();
                VertexTypes.Clear();

                for (int i = 0; i < n; i++)
                {
                    Vertices.Add(new Vector2((float)Utils.RandomDouble(-0.5, 0.5), (float)Utils.RandomDouble(-0.5, 0.5)));
                    VertexTypes.Add((BodyVertexType)Utils.RandomInt(0, (int)BodyVertexType.MaxValue));
                }
            }


            {
                Vector2 center = Vector2.Zero;
                foreach (Vector2 v in Vertices)
                    center += v;
                center /= Vertices.Count;
                for (int i = 0; i < Vertices.Count; i++)
                    Vertices[i] -= center;
                var tmp = Vertices.Zip(VertexTypes).OrderBy(x => Vector2.Dot(x.First, Vector2.UnitX));
                Vertices = tmp.Select(x => x.First).ToList();
                VertexTypes = tmp.Select(x => x.Second).ToList();
            }
        }

        public Gene Mate(Genome genome0, Genome genome1, Gene other, MutationOptions options)
        {
            //For now choose a random brain between the two parents
            if (Utils.RandomDouble() < options.CrossoverChance)
            {
                int crossover_point = Utils.RandomInt(1, Math.Min(this.Vertices.Count, (other as BodyGene).Vertices.Count));
                var bg = new BodyGene();
                for (int i = 0; i < crossover_point; i++)
                {
                    bg.Vertices.Add(Vertices[i]);
                    bg.VertexTypes.Add(VertexTypes[i]);
                }
                for (int i = crossover_point; i < (other as BodyGene).Vertices.Count; i++)
                {
                    bg.Vertices.Add((other as BodyGene).Vertices[i]);
                    bg.VertexTypes.Add((other as BodyGene).VertexTypes[i]);
                }
                return bg;
            }
            else {
                if (Utils.RandomBool())
                    return new BodyGene(this);
                else
                    return new BodyGene(other as BodyGene);
            }
        }

        public Gene Mutate(Genome genome, MutationOptions options)
        {
            BodyGene mutatedGene = new BodyGene();

            //Add noise to the vertices
            Vector2 center = Vector2.Zero;
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vector2 vertex = Vertices[i];

                if (Utils.RandomDouble() < options.MutationChance)
                {
                    vertex.X += (float)Utils.RandomDouble(-options.MutationSize, options.MutationSize);
                    vertex.Y += (float)Utils.RandomDouble(-options.MutationSize, options.MutationSize);
                }
                mutatedGene.Vertices.Add(vertex);

                if (Utils.RandomDouble() < options.MutationChance)
                {
                    mutatedGene.VertexTypes.Add((BodyVertexType)(Utils.RandomInt(0, (int)BodyVertexType.MaxValue)));
                }
                else
                    mutatedGene.VertexTypes.Add(VertexTypes[i]);

                center += vertex;
            }
            if (Utils.RandomDouble() < options.MutationChance)
            {
                Vector2 vertex = Vector2.Zero;
                vertex.X += (float)Utils.RandomDouble(-0.5, 0.5);
                vertex.Y += (float)Utils.RandomDouble(-0.5, 0.5);
                mutatedGene.Vertices.Add(vertex);
                center += vertex;
            }
            center /= Vertices.Count;

            for (int i = 0; i < mutatedGene.Vertices.Count; i++)
            {
                mutatedGene.Vertices[i] -= center;
            }
            var tmp = mutatedGene.Vertices.Zip(mutatedGene.VertexTypes).OrderBy(x => Vector2.Dot(x.First, Vector2.UnitX));
            mutatedGene.Vertices = tmp.Select(x => x.First).ToList();
            mutatedGene.VertexTypes = tmp.Select(x => x.Second).ToList();

            return mutatedGene;
        }
    }
}