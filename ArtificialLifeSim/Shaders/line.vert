#version 460 core

layout(std430, binding = 0) buffer VertexBuffer {
    vec2 position[];
};

layout(location = 1) uniform float zoom;
layout(location = 2) uniform vec2 offset;
layout(location = 4) uniform vec2 size_scale;

void main() {
    vec2 pos = position[gl_VertexID].xy;
    pos = (pos - offset) / zoom;

    pos.x *=  size_scale.y / size_scale.x;
    
    gl_Position = vec4(pos, 0.0, 1.0);
}