// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using ArtificialLifeSim.Chromosomes;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace ArtificialLifeSim.Renderer
{
    class OrganismRenderer : IDisposable
    {
        CircleRenderer EmptyRenderer;
        CircleRenderer EyeRenderer;
        CircleRenderer MouthRenderer;

        PolygonRenderer MuscleRenderer;
        PolygonRenderer StickRenderer;

        private bool disposedValue;

        public OrganismRenderer()
        {
            EyeRenderer = new CircleRenderer();
            EyeRenderer.UpdateColor(new Vector3(1.0f, 1.0f, 0.4f));

            MouthRenderer = new CircleRenderer();
            MouthRenderer.UpdateColor(new Vector3(1.0f, 0.4f, 1.0f));

            EmptyRenderer = new CircleRenderer();
            EmptyRenderer.UpdateColor(new Vector3(0.1f, 0.1f, 0.1f));

            StickRenderer = new PolygonRenderer();
            StickRenderer.UpdateColor(new Vector3(0.5f, 0.5f, 0.5f));

            MuscleRenderer = new PolygonRenderer();
            MuscleRenderer.UpdateColor(new Vector3(1.0f, 0.4f, 0.4f));
        }

        public void Clear()
        {
            EyeRenderer.Clear();
            MouthRenderer.Clear();
            EmptyRenderer.Clear();
            StickRenderer.Clear();
            MuscleRenderer.Clear();
        }

        public void Record(Organism organism)
        {
            var entity = ObjectPool.EntityPool[organism.Body];
            foreach (var stick_i in entity.Sticks)
            {
                var stick = ObjectPool.StickPool[stick_i];
                var n0 = ObjectPool.NodePool[stick.Node0];
                var n1 = ObjectPool.NodePool[stick.Node1];

                if (stick.Type == BodyLinkType.None)
                    StickRenderer.Record(n0.Position, n1.Position, 0.1f * stick.Stiffness + 0.05f);
                else if (stick.Type == BodyLinkType.Muscle)
                    MuscleRenderer.Record(n0.Position, n1.Position, 0.1f * stick.Stiffness + 0.05f);
            }

            ObjectPool.NodePool.AcquireRef();
            try
            {
                foreach (var x in entity.Nodes.Where(x => ObjectPool.NodePool.Get(x).Type == BodyNodeType.Mouth))
                    MouthRenderer.Record(ObjectPool.NodePool.Get(x).Position, World.NodeRadius, ObjectPool.NodePool.Get(x).CurrentFriction * 0.5f + 0.5f);

                foreach (var x in entity.Nodes.Where(x => ObjectPool.NodePool.Get(x).Type == BodyNodeType.Eye))
                    EyeRenderer.Record(ObjectPool.NodePool.Get(x).Position, World.NodeRadius, ObjectPool.NodePool.Get(x).CurrentFriction * 0.5f + 0.5f);

                foreach (var x in entity.Nodes.Where(x => ObjectPool.NodePool.Get(x).Type == BodyNodeType.Empty))
                    EmptyRenderer.Record(ObjectPool.NodePool.Get(x).Position, World.NodeRadius, ObjectPool.NodePool.Get(x).CurrentFriction * 0.5f + 0.5f);
            }
            finally { ObjectPool.NodePool.ReleaseRef(); }
        }

        public void Render()
        {
            MuscleRenderer.Render();
            StickRenderer.Render();

            MouthRenderer.Render();
            EyeRenderer.Render();
            EmptyRenderer.Render();
        }

        public void UpdateView(float zoom, Vector2 pos)
        {
            StickRenderer.UpdateView(zoom, pos);
            MuscleRenderer.UpdateView(zoom, pos);

            EyeRenderer.UpdateView(zoom, pos);
            MouthRenderer.UpdateView(zoom, pos);
            EmptyRenderer.UpdateView(zoom, pos);
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
                // GL.UnmapNamedBuffer(vertexBuffer);
                // GL.DeleteBuffer(vertexBuffer);
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~OrganismRenderer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        internal void UpdateSizeScale(float v, float w)
        {
            StickRenderer.UpdateSizeScale(v, w);
            MuscleRenderer.UpdateSizeScale(v, w);

            EyeRenderer.UpdateSizeScale(v, w);
            MouthRenderer.UpdateSizeScale(v, w);
            EmptyRenderer.UpdateSizeScale(v, w);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}