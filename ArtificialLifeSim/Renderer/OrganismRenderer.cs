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
            EyeRenderer.UpdateColor(new Vector3(0.4f, 0.4f, 0.4f));

            MouthRenderer = new CircleRenderer();
            MouthRenderer.UpdateColor(new Vector3(0.4f, 0.4f, 1.0f));

            EmptyRenderer = new CircleRenderer();
            EmptyRenderer.UpdateColor(new Vector3(0.1f, 0.1f, 0.1f));

            StickRenderer = new PolygonRenderer();
            StickRenderer.UpdateColor(new Vector3(0.5f, 0.5f, 0.0f));

            MuscleRenderer = new PolygonRenderer();
            MuscleRenderer.UpdateColor(new Vector3(1.0f, 0.4f, 0.4f));
        }

        public void Record(Organism organism)
        {
            foreach (var stick in organism.Body.Sticks)
                if (stick.Type == BodyLinkType.None)
                    StickRenderer.Record(stick.Node0.Position, stick.Node1.Position);
                else if (stick.Type == BodyLinkType.Muscle)
                    MuscleRenderer.Record(stick.Node0.Position, stick.Node1.Position);

            organism.Body.Nodes.Where(x => x.Type == BodyNodeType.Mouth).ToList().ForEach(x => MouthRenderer.Record(x.Position, World.NodeRadius));
            organism.Body.Nodes.Where(x => x.Type == BodyNodeType.Eye).ToList().ForEach(x => EyeRenderer.Record(x.Position, World.NodeRadius));
            organism.Body.Nodes.Where(x => x.Type == BodyNodeType.Empty).ToList().ForEach(x => EmptyRenderer.Record(x.Position, World.NodeRadius));
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