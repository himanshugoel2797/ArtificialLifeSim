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

namespace ArtificialLifeSim
{
    class World
    {
        public const float NodeRadius = 0.5f;

        public int Side { get; private set; }
        public List<Organism> Organisms { get; set; }
        public ConcurrentQueue<Organism> NewOrganisms { get; set; }
        public SpatialGrid<Food> Food { get; set; }
        public PhysicsWorld Physics { get; set; }
        public OrganismFactory OrganismFactory { get; set; }

        public double Tick { get; set; }
        public SimWindow Window { get; set; }
        public FoodRenderer FoodRenderer { get; set; }
        public OrganismRenderer OrganismRenderer { get; set; }

        public World(int side, int initialOrganisms, int initialFood)
        {
            #region Renderer Setup
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1024, 1024),
                Title = "Artificial Life",
                Flags = ContextFlags.ForwardCompatible,
            };
            Window = new SimWindow(GameWindowSettings.Default, nativeWindowSettings);
            Window.VSync = VSyncMode.Off;
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.6f, 0.6f, 0.6f, 0.0f);
            GL.BindVertexArray(GL.GenVertexArray());
            GL.LineWidth(50);

            FoodRenderer = new FoodRenderer();
            OrganismRenderer = new OrganismRenderer();
            #endregion

            Side = side;
            OrganismFactory = new OrganismFactory(this);
            Physics = new PhysicsWorld(Side);
            Organisms = new List<Organism>();
            NewOrganisms = new ConcurrentQueue<Organism>();
            Food = new SpatialGrid<Food>(side, side: 32);

            for (int i = 0; i < initialOrganisms; i++)
            {
                var o = OrganismFactory.CreateOrganism();
                Organisms.Add(o);
                Physics.AddEntity(o.Body);
            }

            for (int i = 0; i < initialFood; i++)
                Food.Add(new Food(Utils.RandomVector2(0, side), (float)Utils.RandomDouble(0.01, 1.0)));

            Window.UpdateFrame += Update;
            Window.RenderFrame += Render;
            Window.Run();
        }

        private void Render(FrameEventArgs obj)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            foreach (Food f in Food)
                FoodRenderer.Record(f);

            foreach (Organism o in Organisms)
                OrganismRenderer.Record(o);

            FoodRenderer.Render();
            OrganismRenderer.Render();
            Window.SwapBuffers();
        }

        private void Update(FrameEventArgs obj)
        {
            Run();
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

            //Window.Title = $"Health: {Organism.HighestHealth:F3}, Energy: {Organism.HighestEnergy:F3}, Age: {PreviousHighestAge:F3}, Reproduction Cost: {Organism.HighestParameters.EnergyConsumptionForReproduction:F3}, Reproduction Level: {Organism.HighestParameters.NecessaryEnergyLevelForReproduction:F3}, Reproduction Age: {Organism.HighestParameters.NecessaryEnergyDurationForReproduction:F3}, Asexual Reproduction Chance: {Organism.HighestParameters.AsexualReproductionChance:F3}";
        }

        public void AddOrganism(Organism organism)
        {
            NewOrganisms.Enqueue(organism);
        }

        public Node[] GetOrganismsInContext(Organism o, Node n)
        {
            return Physics.Nodes.SearchNeighborhood(n, n.Position, o.VisionRange);
        }

        public Food[] GetFoodInContext(Organism o, Vector2 position)
        {
            return Food.SearchNeighborhood(null, position, o.VisionRange + NodeRadius);
        }

        public void Run()
        {
            // Update organisms
            Parallel.ForEach(Organisms, (o) => o.Update(Tick));

            // Process food consumption
            Food.RemoveAll(x => x.Energy == 0);

            //Remove dead organisms
            Organisms.RemoveAll(x =>
            {
                var cond = x.Energy <= 0;
                if (cond) Physics.RemoveEntity(x.Body);
                return cond;
            });

            //Run physics step
            Physics.Update((float)Tick);

            // Update tick
            Tick += 0.01;
        }
    }
}