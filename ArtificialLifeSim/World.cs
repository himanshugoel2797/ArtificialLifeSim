// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ArtificialLifeSim.Renderer;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using ArtificialLifeSim.Physics;
using System.Threading;

namespace ArtificialLifeSim
{
    class World
    {
        public const float NodeRadius = 0.5f;

        public int Side { get; private set; }
        public List<Organism> Organisms { get; set; }
        public SpatialHashMap<Food> Food { get; set; }
        public PhysicsWorld Physics { get; set; }
        public OrganismFactory OrganismFactory { get; set; }

        public double Tick { get; set; }
        public SimWindow Window { get; set; }
        public FoodRenderer FoodRenderer { get; set; }
        public OrganismRenderer OrganismRenderer { get; set; }

        public int ElitismCount { get; set; } = 10;
        public int NewChildren { get; set; } = 10;
        public int PopulationCount { get; set; } = 50;
        public int FoodCount { get; set; } = 10000;

        public Thread SimThread { get; set; }
        public bool Pause { get; set; }


        public World(int side)
        {
            #region Renderer Setup
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1024, 1024),
                Title = "Artificial Life",
                Flags = ContextFlags.ForwardCompatible,
            };
            Window = new SimWindow(GameWindowSettings.Default, nativeWindowSettings);
            Window.ZoomState = side / 4.0f;
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.6f, 0.6f, 0.6f, 0.0f);
            GL.BindVertexArray(GL.GenVertexArray());
            GL.LineWidth(50);

            FoodRenderer = new FoodRenderer(FoodCount);
            OrganismRenderer = new OrganismRenderer();
            #endregion

            Side = side;
            OrganismFactory = new OrganismFactory(this);
            Physics = new PhysicsWorld(Side);
            Organisms = new List<Organism>();
            Food = new SpatialHashMap<Food>(side, ObjectPool.FoodPool);

            for (int i = 0; i < PopulationCount; i++)
            {
                var o = OrganismFactory.CreateOrganism();
                Organisms.Add(o);
                Physics.AddEntity(o.Body);
            }

            for (int i = 0; i < FoodCount; i++)
            {
                var food = new Food(Utils.RandomVector2(0, side), (float)Utils.RandomDouble(0.01, 1.0));
                Food.Add(ObjectPool.FoodPool.Allocate(food));
            }

            SimThread = new Thread(UpdateTask);
            SimThread.Start();

            Window.UpdateFrame += Update;
            Window.RenderFrame += Render;
            Window.Run();
        }

        private void Render(FrameEventArgs obj)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            lock (FoodRenderer)
                FoodRenderer.Render();
            lock (OrganismRenderer)
                OrganismRenderer.Render();

            Window.SwapBuffers();
        }

        private void UpdateTask()
        {
            while (!Window.IsExiting)
            {
                if (Tick < 100)
                {
                    if (!Pause)
                        Run();

                    lock (FoodRenderer)
                    {
                        FoodRenderer.Clear();
                        foreach (int f in Food)
                            FoodRenderer.Record(ObjectPool.FoodPool[f]);
                    }

                    lock (OrganismRenderer)
                    {
                        OrganismRenderer.Clear();
                        foreach (Organism o in Organisms)
                            OrganismRenderer.Record(o);
                    }
                }
                else
                {
                    //Score organisms
                    var energy_sum = Organisms.Sum(a => a.Energy);
                    var energy_set = Organisms.Select(a => a.Energy / energy_sum);
                    var scored = Organisms.Zip(energy_set).OrderByDescending(a => a.Second).ToArray();
                    energy_set = scored.Select(a => a.Second);
                    var scored_orgs = scored.Select(a => a.First).ToArray();

                    //Create new generation
                    var elite_orgs = scored_orgs.Take(ElitismCount).ToArray();
                    var n_children = new Organism[PopulationCount - ElitismCount - NewChildren];
                    for (int i = 0; i < n_children.Length; i++)
                    {
                        var parents = Utils.RouletteWheelMany(energy_set, 2);
                        n_children[i] = OrganismFactory.Mate(Organisms[parents[0]], Organisms[parents[1]]);
                    }

                    var new_orgs = new Organism[NewChildren];
                    for (int i = 0; i < new_orgs.Length; i++)
                        new_orgs[i] = OrganismFactory.CreateOrganism();

                    //Remove previous generation
                    foreach (var a in Organisms) a.Dispose();
                    Organisms.Clear();
                    Physics.Clear();

                    //Refresh surviving organisms
                    foreach (var e in elite_orgs) OrganismFactory.RefreshOrganism(e);

                    //Add new generation
                    var n_org_list = new List<Organism>(PopulationCount);
                    n_org_list.AddRange(elite_orgs);
                    n_org_list.AddRange(n_children);
                    n_org_list.AddRange(new_orgs);
                    foreach (var o in n_org_list) Physics.AddEntity(o.Body);
                    Organisms = n_org_list;

                    //Populate food
                    Food.Clear();
                    ObjectPool.FoodPool.Clear();
                    for (int i = 0; i < FoodCount; i++)
                    {
                        var food = new Food(Utils.RandomVector2(0, Side), (float)Utils.RandomDouble(0.01, 1.0));
                        Food.Add(ObjectPool.FoodPool.Allocate(food));
                    }

                    //Restart simulation
                    Tick = 0;
                }
            }
        }

        private void Update(FrameEventArgs obj)
        {
            //Console.Clear();

            Window.ZoomState = Math.Min(Window.ZoomState, Side / 2);
            Window.ZoomState = Math.Max(Window.ZoomState, 1);

            if (Window.IsTargetMode)
            {
                //FoodRenderer.UpdateView(Window.ZoomState, Organism.HighestPosition);
                //OrganismRenderer.UpdateView(Window.ZoomState, Organism.HighestPosition);
            }
            else
            {
                FoodRenderer.UpdateView(Window.ZoomState, Window.ViewPosition + new Vector2(Side / 2, Side / 2));
                OrganismRenderer.UpdateView(Window.ZoomState, Window.ViewPosition + new Vector2(Side / 2, Side / 2));
            }

            FoodRenderer.UpdateSizeScale(Window.XScale, Window.YScale);
            OrganismRenderer.UpdateSizeScale(Window.XScale, Window.YScale);

            Pause ^= Window.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.P);
            Window.Title = $"Tick: {Tick}";
            //Window.Title = $"Health: {Organism.HighestHealth:F3}, Energy: {Organism.HighestEnergy:F3}, Age: {PreviousHighestAge:F3}, Reproduction Cost: {Organism.HighestParameters.EnergyConsumptionForReproduction:F3}, Reproduction Level: {Organism.HighestParameters.NecessaryEnergyLevelForReproduction:F3}, Reproduction Age: {Organism.HighestParameters.NecessaryEnergyDurationForReproduction:F3}, Asexual Reproduction Chance: {Organism.HighestParameters.AsexualReproductionChance:F3}";
        }

        public int[] GetFoodInContext(Organism o, Vector2 position, bool eating)
        {
            if (eating)
                return Food.SearchNeighborhood(position, o.EatingRange + NodeRadius);
            else
                return Food.SearchNeighborhood(position, o.VisionRange + NodeRadius);
        }

        public void Run()
        {
            // Update organisms
            Parallel.ForEach(Organisms, (o) => o.Update(Tick));
            //foreach (Organism o in Organisms) o.Update(Tick);

            // Process food consumption
            Food.FreeAll(x => ObjectPool.FoodPool[x].Energy == 0);

            //Remove dead organisms
            //Organisms.RemoveAll(x =>
            //{
            //    var cond = x.Energy <= 0;
            //    if (cond) Physics.RemoveEntity(x.Body);
            //    return cond;
            //});

            //Run physics step
            Physics.Update((float)Tick);

            // Update tick
            Tick += 0.01;
        }
    }
}