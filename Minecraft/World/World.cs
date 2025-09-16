using OpenTK.Mathematics;

namespace Minecraft.World;

public class World
{
	public Dictionary<Vector3i, Chunk> Chunks = new();
	
	public void Generate()
	{
		for (int x = 0; x < 3; x++)
		{
			for (int z = 0; z < 3; z++)
			{
				Vector3i position = new Vector3i(x, 0, z);
				
				Chunk chunk = new();
				chunk.Generate(position);
				Chunks[position] = chunk;
			}
		}
	}
	
	public Block? GetBlock(Vector3i position)
	{
		Vector3i chunkPos = new Vector3i(
			(int)Math.Floor((double)position.X / Chunk.SizeX),
			(int)Math.Floor((double)position.Y / Chunk.SizeY),
			(int)Math.Floor((double)position.Z / Chunk.SizeZ)
		);
    
		if (!Chunks.ContainsKey(chunkPos))
			return null;
    
		Vector3i localPos = new Vector3i(
			position.X - chunkPos.X * Chunk.SizeX,
			position.Y - chunkPos.Y * Chunk.SizeY,
			position.Z - chunkPos.Z * Chunk.SizeZ
		);
    
		if (localPos.X < 0 || localPos.X >= Chunk.SizeX ||
		    localPos.Y < 0 || localPos.Y >= Chunk.SizeY ||
		    localPos.Z < 0 || localPos.Z >= Chunk.SizeZ)
		{
			return null;
		}
    
		return Chunks[chunkPos].GetBlock(localPos);
	}
}
