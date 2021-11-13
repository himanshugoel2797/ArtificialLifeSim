using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtificialLifeSim.Chromosomes;
using ArtificialLifeSim.Physics;
using OpenTK.Mathematics;

namespace ArtificialLifeSim
{
    class Organism : IDisposable
    {
        private bool disposedValue;

        public World World { get; set; }
        public BodyChromosome BodyChromosome { get; set; }
        public IChromosome[] Genome { get; internal set; }
        public float Energy { get; internal set; }
        public float Age { get; internal set; }
        public float VisionRange { get; internal set; }
        public float EatingRange { get; internal set; }
        public int Body { get; internal set; }

        public Organism(World w)
        {
            World = w;
        }

        public void Setup()
        {

        }

        public void Update(double time)
        {
            var nodes = ObjectPool.EntityPool[Body].Nodes;
            foreach (var node_i in nodes)
            {
                var node = ObjectPool.NodePool[node_i];
                if (node.Type == BodyNodeType.Mouth)
                {
                    //Look for neighboring food
                    var food = World.GetFoodInContext(this, node.Position, eating: true);
                    foreach (var f in food)
                    {
                        ObjectPool.FoodPool.AcquireRef();
                        try
                        {
                            ref var _food = ref ObjectPool.FoodPool.Get(f);
                            lock (_food.Lock)
                                if (_food.Energy != 0)
                                {
                                    Energy += _food.Energy;
                                    _food.Energy = 0;
                                    break;
                                }
                        }
                        finally { ObjectPool.FoodPool.ReleaseRef(); }
                    }
                }
                else if (node.Type == BodyNodeType.Eye)
                {
                    //Look for neighboring food
                    var food = World.GetFoodInContext(this, node.Position, eating: false);
                    foreach (var f in food)
                    {
                        var _food = ObjectPool.FoodPool.Get(f);
                        if (_food.Energy != 0)
                        {
                            //Pull node towards food
                            if (_food.Position != node.Position)
                                node.Position += Vector2.Normalize(_food.Position - node.Position);
                            break;
                        }
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                ObjectPool.EntityPool.Free(Body);
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~Organism()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
