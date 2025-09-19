using Minecraft.Graphics;
using OpenTK.Mathematics;

namespace Minecraft.World;

public class World(WorldRenderer worldRenderer)
{
	public Dictionary<Vector3i, Chunk> Chunks = new();
	private Vector3i _oldCameraPosition = Vector3i.Zero;
	private WorldRenderer _worldRenderer = worldRenderer;

	private void GenerateChunk(Vector3i chunkPosition)
	{
		Chunk chunk = new();
		chunk.Generate(chunkPosition);
		Chunks[chunkPosition] = chunk;
	}
	
	public BlockType GetBlock(Vector3i position)
	{
		Vector3i chunkPos = new Vector3i(
			(int)Math.Floor((double)position.X / Chunk.SizeX),
			(int)Math.Floor((double)position.Y / Chunk.SizeY),
			(int)Math.Floor((double)position.Z / Chunk.SizeZ)
		);
    
		if (!Chunks.ContainsKey(chunkPos))
			return BlockType.Air;
    
		Vector3i localPos = new Vector3i(
			position.X - chunkPos.X * Chunk.SizeX,
			position.Y - chunkPos.Y * Chunk.SizeY,
			position.Z - chunkPos.Z * Chunk.SizeZ
		);
    
		if (localPos.X < 0 || localPos.X >= Chunk.SizeX ||
		    localPos.Y < 0 || localPos.Y >= Chunk.SizeY ||
		    localPos.Z < 0 || localPos.Z >= Chunk.SizeZ)
		{
			return BlockType.Air;
		}
    
		return Chunks[chunkPos].GetBlock(localPos);
	}

	public void CheckAndGenerateNewChunk(Vector3 position)
	{
		Vector3i cameraChunkPosition = new Vector3i(
			(int)(position.X / Chunk.SizeX),
			(int)(position.Y / Chunk.SizeY),
			(int)(position.Z / Chunk.SizeZ));

		if (_oldCameraPosition == cameraChunkPosition) return;
		_oldCameraPosition = cameraChunkPosition;

		for (int x = -3; x <= 3; x++)
		{
			for (int z = -3; z <= 3; z++)
			{
				Vector3i chunkPosition = cameraChunkPosition + new Vector3i(x, 0, z);
				if (Chunks.ContainsKey(chunkPosition))
					continue;
				
				GenerateChunk(chunkPosition);
				_worldRenderer.RebuildChunkMesh(this, chunkPosition);
			}
		}
	}
}
