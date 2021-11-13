// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace ArtificialLifeSim.Renderer {
    class CircleRenderer : IDisposable{

        int BufferLen;

        Shader shader;
        BufferHandle circleGeom;
        BufferHandle vertexBuffer;
        IntPtr[] vertexBufferPtr;
        int[] pointCnts;
        IntPtr curVbufPtr;
        int circleGeomLen = 0;
        int vbuf_idx = 0;
        int maxPointCnt = 0;
        private bool disposedValue;

        public CircleRenderer(int buf_size = 10240) {
            shader = new Shader("Shaders/circle.vert", "Shaders/circle.frag");

            BufferLen = buf_size * 32 * 4 * sizeof(float);
            maxPointCnt = buf_size * 32;
            circleGeom = GL.CreateBuffer();
            {
                int circleSteps = 10;
                float[] circle_geom = new float[circleSteps * 3 * 2];

                float angleStep = (float)(2 * Math.PI / circleSteps);
                for (int i = 0; i < circleSteps; i++)
                {
                    circle_geom[i * 6 + 0] = (float)Math.Sin(i * angleStep);
                    circle_geom[i * 6 + 1] = (float)Math.Cos(i * angleStep);

                    circle_geom[i * 6 + 2] = 0;
                    circle_geom[i * 6 + 3] = 0;

                    circle_geom[i * 6 + 4] = (float)Math.Sin((i + 1) * angleStep);
                    circle_geom[i * 6 + 5] = (float)Math.Cos((i + 1) * angleStep);
                }
                circleGeomLen = circle_geom.Length;

                GL.NamedBufferData(circleGeom, circle_geom, VertexBufferObjectUsage.StaticDraw);
            }

            vertexBuffer = GL.CreateBuffer();
            GL.NamedBufferStorage(vertexBuffer, 3 * BufferLen, IntPtr.Zero, BufferStorageMask.MapPersistentBit | BufferStorageMask.MapWriteBit | BufferStorageMask.MapCoherentBit);
            unsafe
            {
                var ptr = GL.MapNamedBufferRange(vertexBuffer, IntPtr.Zero, 3 * BufferLen, MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapPersistentBit | MapBufferAccessMask.MapCoherentBit);
                vertexBufferPtr = new IntPtr[] {(IntPtr)ptr, (IntPtr)ptr + BufferLen, (IntPtr)ptr + 2 * BufferLen};
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

        public void Record(Vector2 position, float radius, float opacity = 0.5f){
            unsafe {
                var ptr = (float*)curVbufPtr;
                ptr[0] = position.X;
                ptr[1] = position.Y;
                ptr[2] = radius;
                ptr[3] = opacity;
                curVbufPtr += 4 * sizeof(float);
            }
            pointCnts[vbuf_idx]++;
            if (pointCnts[vbuf_idx] >= maxPointCnt)
                throw new Exception();
        }

        public void Render(){
            shader.Activate();
            GL.BindBufferRange(BufferTargetARB.ShaderStorageBuffer, 0, vertexBuffer, (IntPtr)(vbuf_idx * BufferLen), BufferLen);
            GL.BindBufferRange(BufferTargetARB.ShaderStorageBuffer, 1, circleGeom, IntPtr.Zero, circleGeomLen * sizeof(float));
            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, circleGeomLen / 2, pointCnts[vbuf_idx]);

            vbuf_idx = (vbuf_idx + 1) % 3;
            curVbufPtr = vertexBufferPtr[vbuf_idx];
            pointCnts[vbuf_idx] = 0;
        }

        public void UpdateView(float zoom, Vector2 center){
            GL.ProgramUniform1f(shader.ProgramID, 1, zoom);
            GL.ProgramUniform2f(shader.ProgramID, 2, center.X, center.Y);
        }

        public void UpdateColor(Vector3 color){
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
        ~CircleRenderer()
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