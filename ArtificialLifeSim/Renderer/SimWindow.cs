// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ArtificialLifeSim.Renderer {
    class SimWindow : GameWindow {
        public int ZoomState = 1;
        public float XScale = 1;
        public float YScale = 1;
        public int RefSizeX = 1024;
        public int RefSizeY = 1024;
        public bool IsTargetMode = false;

        public Vector2 ViewPosition = new Vector2(0, 0);
        public SimWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) 
        {
            RefSizeX = nativeWindowSettings.Size.X;
            RefSizeY = nativeWindowSettings.Size.Y;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();

            if (KeyboardState.IsKeyDown(Keys.Q))
                ZoomState+=1;
            if (KeyboardState.IsKeyDown(Keys.E))
                ZoomState-=1;
                
            if (KeyboardState.IsKeyDown(Keys.A))
                ViewPosition.X -= 0.3f;
            if (KeyboardState.IsKeyDown(Keys.D))
                ViewPosition.X += 0.3f;
            if (KeyboardState.IsKeyDown(Keys.W))
                ViewPosition.Y += 0.3f;
            if (KeyboardState.IsKeyDown(Keys.S))
                ViewPosition.Y -= 0.3f;

            if (KeyboardState.IsKeyPressed(Keys.D1))
                IsTargetMode = !IsTargetMode;

            base.OnUpdateFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            XScale = e.Width / (float)RefSizeX;
            YScale = e.Height / (float)RefSizeY;
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }
    }
}