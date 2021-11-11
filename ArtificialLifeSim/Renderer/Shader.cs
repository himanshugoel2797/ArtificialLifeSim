// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace ArtificialLifeSim.Renderer
{
    class Shader : IDisposable
    {
        private bool disposedValue;

        public OpenTK.Graphics.ProgramHandle ProgramID { get; private set; }

        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexCode = File.ReadAllText(vertexPath);
            string fragmentCode = File.ReadAllText(fragmentPath);

            var vertShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertShader, vertexCode);
            GL.CompileShader(vertShader);
            unsafe {
                int success;
                GL.GetShaderiv(vertShader, ShaderParameterName.CompileStatus, &success);

                if(success == 0)
                {
                    GL.GetShaderInfoLog(vertShader, out string infoLog);
                    System.Console.WriteLine($"Vertex Shader Compilation Error: {infoLog}");
                }
            }

            var fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, fragmentCode);
            GL.CompileShader(fragShader);
            unsafe {
                int success;
                GL.GetShaderiv(fragShader, ShaderParameterName.CompileStatus, &success);

                if(success == 0)
                {
                    GL.GetShaderInfoLog(fragShader, out string infoLog);
                    System.Console.WriteLine($"Fragment Shader Compilation Error: {infoLog}");
                }
            }

            ProgramID = GL.CreateProgram();
            GL.AttachShader(ProgramID, vertShader);
            GL.AttachShader(ProgramID, fragShader);
            GL.LinkProgram(ProgramID);
            
            unsafe {
                int success;
                GL.GetProgramiv(ProgramID, ProgramPropertyARB.LinkStatus, &success);

                if(success == 0)
                {
                    GL.GetProgramInfoLog(ProgramID, out string infoLog);
                    System.Console.WriteLine($"Program Linking Error: {infoLog}");
                }
            }

            GL.DetachShader(ProgramID, vertShader);
            GL.DetachShader(ProgramID, fragShader);
            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);
        }

        public void Activate()
        {
            GL.UseProgram(ProgramID);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                GL.DeleteProgram(ProgramID);
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~Shader()
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