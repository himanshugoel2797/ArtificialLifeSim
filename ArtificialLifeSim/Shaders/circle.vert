#version 460 core

layout(std430, binding = 0) buffer VertexBuffer {
    vec4 position[];
};

layout(std430, binding = 1) buffer GeomBuffer {
    vec2 geom[];
};

layout(location = 1) uniform float zoom;
layout(location = 2) uniform vec2 offset;
layout(location = 4) uniform vec2 size_scale;

void main() {
    vec2 pos = position[gl_InstanceID].xy;
    vec2 vert = geom[gl_VertexID];
    pos = (vert * position[gl_InstanceID].z + pos - offset) / zoom;

    pos.x *=  size_scale.y / size_scale.x;

    gl_Position = vec4(pos, 0.0, 1.0);
}