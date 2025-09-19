#version 410 core

out vec4 FragColor;

in vec2 TexCoord;
in vec3 Color;
in float BlockId;
in vec2 MeshSize;

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

	vec4 t = texture(uTexture, atlasCoord);
	FragColor = vec4(Color * t.rgb, t.a);
	// FragColor = vec4(TexCoord, 0.0, 1.0);
}
