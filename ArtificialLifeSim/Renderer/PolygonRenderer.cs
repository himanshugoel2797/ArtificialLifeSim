// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Matrix22 = OpenTK.Mathematics.Matrix2;

namespace ArtificialLifeSim.Renderer
{
    class PolygonRenderer : IDisposable
    {

        const int BufferLen = 10240 * 32 * 12 * sizeof(float);

        Shader shader;
        BufferHandle vertexBuffer;
        IntPtr[] vertexBufferPtr;
        int[] pointCnts;
        IntPtr curVbufPtr;
        int vbuf_idx = 0;
        int maxPointCnt = BufferLen / (12 * sizeof(float));
        private bool disposedValue;

        public PolygonRenderer()
        {

            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(5.0f);

            shader = new Shader("Shaders/line.vert", "Shaders/line.frag");
            vertexBuffer = GL.CreateBuffer();
            GL.NamedBufferStorage(vertexBuffer, 3 * BufferLen, IntPtr.Zero, BufferStorageMask.MapPersistentBit | BufferStorageMask.MapWriteBit | BufferStorageMask.MapCoherentBit);
            unsafe
            {
                var ptr = GL.MapNamedBufferRange(vertexBuffer, IntPtr.Zero, 3 * BufferLen, MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapPersistentBit | MapBufferAccessMask.MapCoherentBit);
                vertexBufferPtr = new IntPtr[] { (IntPtr)ptr, (IntPtr)ptr + BufferLen, (IntPtr)ptr + 2 * BufferLen };
                pointCnts = new int[3];
                vbuf_idx = 0;
                curVbufPtr = vertexBufferPtr[vbuf_idx];
            }
        }

        internal void UpdateSizeScale(float v, float w)
        {
            GL.ProgramUniform2f(shader.ProgramID, 4, v, w);
        }

        public void Clear()
        {
            //vbuf_idx = (vbuf_idx + 1) % 3;
            curVbufPtr = vertexBufferPtr[vbuf_idx];
            pointCnts[vbuf_idx] = 0;
        }

        public void Record(Vector2 p0, Vector2 p1, float width = 0.1f)
        {
            var dir = Vector2.Normalize(p1 - p0);
            p0 += dir * World.NodeRadius * 0.5f;
            p1 -= dir * World.NodeRadius * 0.5f;

            var d0 = dir.PerpendicularClockwise();
            var c0 = p0 + d0 * width;
            var c1 = p0 - d0 * width;
            var c2 = p1 + d0 * width;
            var c3 = p1 - d0 * width;

            unsafe
            {
                var ptr = (float*)curVbufPtr;
                ptr[0] = c0.X;
                ptr[1] = c0.Y;
                ptr[2] = c1.X;
                ptr[3] = c1.Y;
                ptr[4] = c2.X;
                ptr[5] = c2.Y;
                
                ptr[6] = c2.X;
                ptr[7] = c2.Y;
                ptr[8] = c3.X;
                ptr[9] = c3.Y;
                ptr[10] = c1.X;
                ptr[11] = c1.Y;
                curVbufPtr += 12 * sizeof(float);
                pointCnts[vbuf_idx]++;
                if (pointCnts[vbuf_idx] >= maxPointCnt)
                    throw new Exception();
            }
        }

        public void Render()
        {
            shader.Activate();
            GL.BindBufferRange(BufferTargetARB.ShaderStorageBuffer, 0, vertexBuffer, (IntPtr)(vbuf_idx * BufferLen), BufferLen);
            GL.DrawArrays(PrimitiveType.Triangles, 0, pointCnts[vbuf_idx] * 6);

            vbuf_idx = (vbuf_idx + 1) % 3;
            curVbufPtr = vertexBufferPtr[vbuf_idx];
            pointCnts[vbuf_idx] = 0;
        }

        public void UpdateView(float zoom, Vector2 center)
        {
            GL.ProgramUniform1f(shader.ProgramID, 1, zoom);
            GL.ProgramUniform2f(shader.ProgramID, 2, center.X, center.Y);
        }

        public void UpdateColor(Vector3 color)
        {
            GL.ProgramUniform3f(shader.ProgramID, 3, color.X, color.Y, color.Z);
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
                GL.UnmapNamedBuffer(vertexBuffer);
                GL.DeleteBuffer(vertexBuffer);
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~PolygonRenderer()
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