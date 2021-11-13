// Copyright (c) 2021 Himanshu Goel
// 
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using OpenTK.Mathematics;
using System;

namespace ArtificialLifeSim.Renderer {
    class FoodRenderer{

        CircleRenderer circleRenderer;

        public FoodRenderer(int food_cnt) {
            circleRenderer = new CircleRenderer(food_cnt);
            circleRenderer.UpdateColor(new Vector3(0.0f, 0.5f, 0.0f));
        }

        public void UpdateView(float zoom, Vector2 center) {
            circleRenderer.UpdateView(zoom, center);
        }

        public void Clear()
        {
            circleRenderer.Clear();
        }

        public void Record(Food f){
            circleRenderer.Record(f.Position, 3.0f);
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