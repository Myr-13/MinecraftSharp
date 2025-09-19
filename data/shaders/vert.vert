#version 410 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aBlockId;
layout (location = 3) in vec3 aColor;
layout (location = 4) in vec2 aMeshSize;

out vec2 TexCoord;
out vec3 Color;
out float BlockId;
out vec2 MeshSize;

uniform mat4 view;
uniform mat4 projection;

void main()
{
	gl_Position = projection * view * mat4(1.0) * vec4(aPos, 1.0);
	TexCoord = aTexCoord;
	BlockId = aBlockId;
	Color = aColor;
	MeshSize = aMeshSize;
}
