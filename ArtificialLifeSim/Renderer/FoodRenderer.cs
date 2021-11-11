// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using System.Numerics;

namespace ArtificialLifeSim.Renderer {
    class FoodRenderer{

        CircleRenderer circleRenderer;

        public FoodRenderer() {
            circleRenderer = new CircleRenderer();
            circleRenderer.UpdateColor(new Vector3(0.0f, 1.0f, 0.0f));
        }

        public void UpdateView(float zoom, Vector2 center) {
            circleRenderer.UpdateView(zoom, center);
        }

        public void Record(Food f){
            circleRenderer.Record(f.Position, f.Radius);
        }


        public void Render(){
            circleRenderer.Render();
        }

        internal void UpdateSizeScale(float v, float w)
        {
            circleRenderer.UpdateSizeScale(v, w);
        }
    }
}