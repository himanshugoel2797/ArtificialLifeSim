// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Numerics;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Matrix22 = OpenTK.Mathematics.Matrix2;

namespace ArtificialLifeSim.Renderer {
    class PolygonRenderer : IDisposable{

        const int BufferLen = 102400 * OrganismLimits.MaxVertexCount * 4 * sizeof(float);

        Shader shader;
        BufferHandle vertexBuffer;
        IntPtr[] vertexBufferPtr;
        IntPtr curVbufPtr;
        int vbuf_idx = 0;
        int pointCnt = 0;
        private bool disposedValue;

        public PolygonRenderer() {

            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(5.0f);

            shader = new Shader("Shaders/line.vert", "Shaders/line.frag");
            vertexBuffer = GL.CreateBuffer();
            GL.NamedBufferStorage(vertexBuffer, 3 * BufferLen, IntPtr.Zero, BufferStorageMask.MapPersistentBit | BufferStorageMask.MapWriteBit | BufferStorageMask.MapCoherentBit);
            unsafe
            {
                var ptr = GL.MapNamedBufferRange(vertexBuffer, IntPtr.Zero, 3 * BufferLen, MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapPersistentBit | MapBufferAccessMask.MapCoherentBit);
                vertexBufferPtr = new IntPtr[] {(IntPtr)ptr, (IntPtr)ptr + BufferLen, (IntPtr)ptr + 2 * BufferLen};
                vbuf_idx = 0;
                curVbufPtr = vertexBufferPtr[vbuf_idx];
            }
        }

        internal void UpdateSizeScale(float v, float w)
        {
            GL.ProgramUniform2f(shader.ProgramID, 4, v, w);
        }

        public void Record(Vector2 b_pos, Vector2[] position, float radius){
            
            unsafe {
                for (int i = 0; i < position.Length; i++)
                {
                    var tmp = position[i];
                    var ptr = (float*)curVbufPtr;
                    ptr[0] = b_pos.X + tmp.X;
                    ptr[1] = b_pos.Y + tmp.Y;

                    tmp = position[(i + 1) % position.Length];
                    ptr[2] = b_pos.X + tmp.X;
                    ptr[3] = b_pos.Y + tmp.Y;
                    curVbufPtr += 4 * sizeof(float);
                    pointCnt++;
                }
            }
        }

        public void Render(){
            shader.Activate();
            GL.BindBufferRange(BufferTargetARB.ShaderStorageBuffer, 0, vertexBuffer, (IntPtr)(vbuf_idx * BufferLen), BufferLen);
            GL.DrawArrays(PrimitiveType.Lines, 0, pointCnt * 2);

            pointCnt = 0;
            vbuf_idx = (vbuf_idx + 1) % 3;
            curVbufPtr = vertexBufferPtr[vbuf_idx];
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