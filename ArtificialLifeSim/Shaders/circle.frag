#version 460 core

layout(location = 0) out vec4 outColor;

layout(location = 3) uniform vec3 color;
layout(location = 4) uniform vec2 size_scale;

in float opacity;

void main() {
    outColor = vec4(color, opacity);
}