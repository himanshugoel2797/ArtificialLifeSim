// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using ArtificialLifeSim.Features;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace ArtificialLifeSim.Renderer
{
    class OrganismRenderer : IDisposable
    {
        CircleRenderer EmptyRenderer;
        CircleRenderer MuscleRenderer;
        CircleRenderer EyeRenderer;
        CircleRenderer RotatorRenderer;
        CircleRenderer MouthRenderer;
        CircleRenderer HullRenderer;
        PolygonRenderer PolygonRenderer;

        private bool disposedValue;

        public OrganismRenderer()
        {
            MuscleRenderer = new CircleRenderer();
            MuscleRenderer.UpdateColor(new Vector3(1.0f, 0.4f, 0.4f));
            
            EyeRenderer = new CircleRenderer();
            EyeRenderer.UpdateColor(new Vector3(0.4f, 0.4f, 0.4f));

            RotatorRenderer = new CircleRenderer();
            RotatorRenderer.UpdateColor(new Vector3(0.7f, 0.4f, 0.7f));
            
            MouthRenderer = new CircleRenderer();
            MouthRenderer.UpdateColor(new Vector3(0.4f, 0.4f, 1.0f));
            
            EmptyRenderer = new CircleRenderer();
            EmptyRenderer.UpdateColor(new Vector3(0.1f, 0.1f, 0.1f));

            HullRenderer = new CircleRenderer();
            HullRenderer.UpdateColor(new Vector3(0.7f, 0.7f, 0.7f));

            PolygonRenderer = new PolygonRenderer();
            PolygonRenderer.UpdateColor(new Vector3(0.5f, 0.5f, 0.0f));
        }

        public void Record(Organism organism)
        {
            //PolygonRenderer.Record(organism.Position, organism.Body.Nodes.Select(a => a.RotatedPosition).ToArray(), 0.1f);

            organism.Body.Nodes.Where(x => x.VertexType == BodyVertexType.Mouth).ToList().ForEach(x => MouthRenderer.Record(organism.Position + x.RotatedPosition, 0.1f));
            organism.Body.Nodes.Where(x => x.VertexType == BodyVertexType.Eye).ToList().ForEach(x => EyeRenderer.Record(organism.Position + x.RotatedPosition, 0.1f));
            organism.Body.Nodes.Where(x => x.VertexType == BodyVertexType.Muscle).ToList().ForEach(x => MuscleRenderer.Record(organism.Position + x.RotatedPosition, 0.1f));
            organism.Body.Nodes.Where(x => x.VertexType == BodyVertexType.Empty).ToList().ForEach(x => EmptyRenderer.Record(organism.Position + x.RotatedPosition, 0.05f));
            HullRenderer.Record(organism.Position, organism.Radius);
        }

        public void Render()
        {
            HullRenderer.Render();
            MouthRenderer.Render();
            EyeRenderer.Render();
            EmptyRenderer.Render();
            MuscleRenderer.Render();
            RotatorRenderer.Render();
            //PolygonRenderer.Render();

            // shader.Activate();
            // GL.BindBufferRange(BufferTargetARB.ArrayBuffer, 0, vertexBuffer, IntPtr.Zero, 1024 * OrganismLimits.MaxVertexCount * 2 * sizeof(float));

            // vbuf_idx = (vbuf_idx + 1) % 3;
            // curVbufPtr = vertexBufferPtr[vbuf_idx];
        }

        public void UpdateView(float zoom, Vector2 pos){
            HullRenderer.UpdateView(zoom, pos);
            PolygonRenderer.UpdateView(zoom, pos);
            MuscleRenderer.UpdateView(zoom, pos);
            EyeRenderer.UpdateView(zoom, pos);
            RotatorRenderer.UpdateView(zoom, pos);
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
            HullRenderer.UpdateSizeScale(v, w);
            PolygonRenderer.UpdateSizeScale(v, w);
            MuscleRenderer.UpdateSizeScale(v, w);
            EyeRenderer.UpdateSizeScale(v, w);
            RotatorRenderer.UpdateSizeScale(v, w);
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