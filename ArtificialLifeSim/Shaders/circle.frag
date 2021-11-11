#version 460 core

layout(location = 0) out vec4 outColor;

layout(location = 3) uniform vec3 color;
layout(location = 4) uniform vec2 size_scale;

void main() {
    outColor = vec4(color, 0.5f);
}