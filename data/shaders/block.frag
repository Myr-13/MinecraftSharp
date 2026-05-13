#version 410 core

out vec4 FragColor;

in vec2 TexCoord;
in vec3 Color;
in float BlockId;
in vec2 MeshSize;
in float Face;

uniform sampler2D uTexture;

void main()
{
	const float tileSize = 16.0 / 256.0;
    const float tilesPerRow = 16.0;

	float col = mod(BlockId, tilesPerRow);
	float row = floor(BlockId / tilesPerRow);

	vec2 atlasCoord = vec2(
		(col + mod(TexCoord.x * MeshSize.x, 1.0)) * tileSize,
		(row + mod(TexCoord.y * MeshSize.y, 1.0)) * tileSize
	);

	float shading = 1.0f;
	if(int(Face) == 1)
		shading = 0.45f;
	else if(int(Face) > 1)
		shading = 0.75f;

	vec4 t = texture(uTexture, atlasCoord);
	FragColor = vec4(Color * t.rgb * shading, t.a);
}
