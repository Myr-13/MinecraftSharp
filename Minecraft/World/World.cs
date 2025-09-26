using System.Diagnostics;
using Minecraft.Graphics;
using OpenTK.Mathematics;

namespace Minecraft.World;

public class World(WorldRenderer worldRenderer)
{
	public Dictionary<Vector3i, Chunk> Chunks = new();
	private Vector3i _oldCameraPosition = Vector3i.Zero;
	private WorldRenderer _worldRenderer = worldRenderer;

	public void GenerateChunk(Vector3i chunkPosition)
	{
		Chunk chunk = new();
		chunk.Generate(chunkPosition);
		Chunks[chunkPosition] = chunk;
		_worldRenderer.RebuildChunkMesh(this, chunkPosition);
	}
	
	public BlockType GetBlock(Vector3i position)
	{
		Vector3i chunkPos = MathUtils.FloorVector(position / Chunk.ChunkSize);
    
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

	public void SetBlock(Vector3i position, BlockType block)
	{
		Vector3i chunkPos = MathUtils.FloorVector(position / Chunk.ChunkSize);
		
		if (!Chunks.ContainsKey(chunkPos))
			return;
    
		Vector3i localPos = new Vector3i(
			position.X - chunkPos.X * Chunk.SizeX,
			position.Y - chunkPos.Y * Chunk.SizeY,
			position.Z - chunkPos.Z * Chunk.SizeZ
		);
    
		if (localPos.X < 0 || localPos.X >= Chunk.SizeX ||
		    localPos.Y < 0 || localPos.Y >= Chunk.SizeY ||
		    localPos.Z < 0 || localPos.Z >= Chunk.SizeZ)
		{
			return;
		}
    
		Chunks[chunkPos].SetBlock(localPos, block);
		_worldRenderer.RebuildChunkMesh(this, chunkPos);
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
			}
		}
	}

	public Vector3i? IntersectLine(Vector3 start, Vector3 end, ref Vector3i prevBlock)
	{
		Vector3 dir = (end - start).Normalized();
		int dist = (int)Math.Floor(Vector3.Distance(start, end) * 10);

		for (int i = 0; i < dist; i++)
		{
			prevBlock = MathUtils.FloorVector(start);
			start += dir / 10;
			Vector3i blockPos = MathUtils.FloorVector(start);
			BlockType type = GetBlock(blockPos);

			if (type != BlockType.Air)
			{
				return blockPos;
			}
		}
		
		return null;
	}
}
