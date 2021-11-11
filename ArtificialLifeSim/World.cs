// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ArtificialLifeSim.Renderer;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace ArtificialLifeSim
{
    class World
    {
        public int Side { get; private set; }
        public MutationOptions MutationOptions { get; private set; }
        public SpatialList<Organism> Organisms { get; set; }
        public ConcurrentQueue<Organism> NewOrganisms { get; set; }
        public SpatialList<Food> Food { get; set; }
        public float BirthRate { get; set; } = 0.01f;   // Probability of spontaneous unsourced birth
        public int MaxPopulation { get; set; } = 1000;
        public float FoodRate { get; set; }
        public float FoodRateDecayRate { get; set; } //The rate at which the natural food rate decays
        public ulong Tick { get; set; }
        public ulong IterCount { get; set; }
        public ulong RunRatio { get; set; } = 1;
        public SimWindow Window { get; set; }
        public FoodRenderer FoodRenderer { get; set; }
        public OrganismRenderer OrganismRenderer { get; set; }

        public int MaxFood { get; set; } = 10000;

        public World(int side, float birthRate, float foodRate, float foodRateDecayRate, int initialOrganisms, int initialFood)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new OpenTK.Mathematics.Vector2i(1024, 1024),
                Title = "Artificial Life",
                Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible,
            };
            Window = new SimWindow(GameWindowSettings.Default, nativeWindowSettings);
            Window.VSync = VSyncMode.Off;
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.6f, 0.6f, 0.6f, 0.0f);
            GL.BindVertexArray(GL.GenVertexArray());

            FoodRenderer = new FoodRenderer();
            OrganismRenderer = new OrganismRenderer();

            MutationOptions = new MutationOptions()
            {

            };

            Side = side;
            Organisms = new SpatialList<Organism>(side, side: 32);
            NewOrganisms = new ConcurrentQueue<Organism>();
            Food = new SpatialList<Food>(side, side: 32);
            BirthRate = birthRate;
            FoodRate = foodRate;
            FoodRateDecayRate = foodRateDecayRate;

            for (int i = 0; i < initialOrganisms; i++)
            {
                var org = new Organism(this, Utils.RandomVector2(0, side));
                org.Generate();
                Organisms.Add(org);
            }
            for (int i = 0; i < initialFood; i++)
            {
                Food.Add(new Food(Utils.RandomVector2(0, side), (float)Utils.RandomDouble(0.01, FoodRateDecayRate)));
            }

            Window.UpdateFrame += Update;
            Window.RenderFrame += Render;
            Window.Run();
        }

        private void Render(FrameEventArgs obj)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            foreach (Food f in Food)
            {
                FoodRenderer.Record(f);
            }

            foreach (Organism o in Organisms)
            {
                OrganismRenderer.Record(o);
            }

            FoodRenderer.Render();
            OrganismRenderer.Render();
            Window.SwapBuffers();
        }

        float PreviousHighestAge = 0;
        private void Update(FrameEventArgs obj)
        {
            Run();
            Window.ZoomState = Math.Min(Window.ZoomState, Side / 2);
            Window.ZoomState = Math.Max(Window.ZoomState, 1);

            if (Window.IsTargetMode)
            {
                FoodRenderer.UpdateView(Window.ZoomState, Organism.HighestPosition);
                OrganismRenderer.UpdateView(Window.ZoomState, Organism.HighestPosition);
            }
            else
            {
                FoodRenderer.UpdateView(Window.ZoomState, Window.ViewPosition + new Vector2(Side / 2, Side / 2));
                OrganismRenderer.UpdateView(Window.ZoomState, Window.ViewPosition + new Vector2(Side / 2, Side / 2));
            }

            FoodRenderer.UpdateSizeScale(Window.XScale, Window.YScale);
            OrganismRenderer.UpdateSizeScale(Window.XScale, Window.YScale);

            Window.Title = $"Health: {Organism.HighestHealth:F3}, Energy: {Organism.HighestEnergy:F3}, Age: {PreviousHighestAge:F3}, Reproduction Cost: {Organism.HighestParameters.EnergyConsumptionForReproduction:F3}, Reproduction Level: {Organism.HighestParameters.NecessaryEnergyLevelForReproduction:F3}, Reproduction Age: {Organism.HighestParameters.NecessaryEnergyDurationForReproduction:F3}, Asexual Reproduction Chance: {Organism.HighestParameters.AsexualReproductionChance:F3}";
            PreviousHighestAge = Organism.HighestAge;
        }

        public void AddOrganism(Organism organism)
        {
            NewOrganisms.Enqueue(organism);
        }

        public Organism[] GetOrganismsInContext(Organism o, Vector2 position)
        {
            return Organisms.SearchNeighborhood(o, position, MathF.Pow(MathF.Sqrt(o.Hull.RadiusSquared) + MathF.Sqrt(o.Parameters.VisionRangeSquared), 2));
        }

        public Food[] GetFoodInContext(Organism o, Vector2 position, bool fromEye = true)
        {
            return Food.SearchNeighborhood(null, position, !fromEye ? o.Hull.RadiusSquared : MathF.Pow(MathF.Sqrt(o.Hull.RadiusSquared) + MathF.Sqrt(o.Parameters.VisionRangeSquared), 2));
        }

        public void ProcessCollisions()
        {
            foreach (Organism organism in Organisms)
            {
                var collisionTargets = Organisms.SearchNeighborhood(organism, organism.Position, 16);
                foreach (Organism otherOrganism in collisionTargets)
                {
                    if ((organism.Position - otherOrganism.Position).LengthSquared() <= MathF.Pow(organism.Hull.Radius + otherOrganism.Hull.Radius, 2))
                    {
                        Physics.Hull.Collide(organism.Hull, otherOrganism.Hull);
                    }
                }
            }
        }

        public void Run()
        {
            if (IterCount % 100 == 0)
                Console.WriteLine(Food.Count);

            // Generate food
            if (Food.Count < MaxFood)
            {
                int n = Utils.RandomInt(1, 10);
                for (int i = 0; i < n; i++)
                    if (FoodRate > Utils.RandomDouble())
                        Food.Add(new Food(Utils.RandomVector2(0, Side), (float)Utils.RandomDouble(0.01, FoodRateDecayRate)));
            }

            if (Utils.RandomDouble() < BirthRate && Organisms.Count < MaxPopulation)
            {
                var org = new Organism(this, Utils.RandomVector2(0, Side));
                org.Generate();
                Organisms.Add(org);
            }

            if (IterCount % RunRatio == 0)
            {
                Organism.HighestAge = 0;
                Organism.HighestEnergy = 0;

                float tickState = (Tick % 2 == 0) ? 1 : -1;

                // Decay food rate
                //FoodRate *= FoodRateDecayRate;

                // Update organisms
                //TODO: Make this parallel
                Organisms.ParallelForEach((o) => o.Update(tickState));
                //foreach (Organism o in Organisms) {
                //    o.Update(tickState);
                //}

                // Update tick
                Tick++;
            }

            // TODO: Process attacks
            // Process food consumption
            Food.RemoveAll(x => x.Energy == 0);
            Organisms.RemoveAll(x => x.Health <= 0);

            foreach (Organism o in Organisms)
                o.Hull.Update(0.01f);


            //Update spatial binning for organisms
            Organisms.RebuildAll();
            ProcessCollisions();

            // Add new organisms
            foreach (var o in NewOrganisms)
                Organisms.Add(o);
            NewOrganisms.Clear();


            IterCount++;
        }
    }
}