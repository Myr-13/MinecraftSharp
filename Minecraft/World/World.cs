using OpenTK.Mathematics;

namespace Minecraft.World;

public class World : IDisposable
{
    public Dictionary<Vector3i, Chunk> Chunks = new();
    private Vector3i _oldCameraPosition = Vector3i.Zero;
    private HashSet<Vector3i> _generatingChunks = new();
    private ChunkGenerator _chunkGenerator = new();
    public const int RenderDistance = 12;

    public void GenerateChunk(Vector3i chunkPosition)
    {
        if (Chunks.ContainsKey(chunkPosition) || _generatingChunks.Contains(chunkPosition))
            return;

        _generatingChunks.Add(chunkPosition);
        _chunkGenerator.EnqueueTask(new ChunkGenerationTask { ChunkPosition = chunkPosition });
    }

    public void ProcessGenerationResults()
    {
        while (_chunkGenerator.TryDequeueResult(out ChunkGenerationResult result))
        {
            _generatingChunks.Remove(result.ChunkPosition);

            if (result.Success && result.Chunk != null)
                Chunks[result.ChunkPosition] = result.Chunk;
        }
    }

    public BlockType GetBlock(Vector3i position)
    {
        Vector3i chunkPos = GetChunkPos(position);

        if (!Chunks.TryGetValue(chunkPos, out Chunk chunk))
            return BlockType.Air;

        Vector3i localPos = new Vector3i(
            position.X - chunkPos.X * Chunk.SizeX,
            position.Y - chunkPos.Y * Chunk.SizeY,
            position.Z - chunkPos.Z * Chunk.SizeZ
        );

        if (localPos.X < 0 || localPos.X >= Chunk.SizeX ||
            localPos.Y < 0 || localPos.Y >= Chunk.SizeY ||
            localPos.Z < 0 || localPos.Z >= Chunk.SizeZ)
            return BlockType.Air;

        return chunk.GetBlock(localPos);
    }

    public void SetBlock(Vector3i position, BlockType block)
    {
        Vector3i chunkPos = GetChunkPos(position);

        if (!Chunks.TryGetValue(chunkPos, out Chunk chunk))
            return;

        Vector3i localPos = new Vector3i(
            position.X - chunkPos.X * Chunk.SizeX,
            position.Y - chunkPos.Y * Chunk.SizeY,
            position.Z - chunkPos.Z * Chunk.SizeZ
        );

        if (localPos.X < 0 || localPos.X >= Chunk.SizeX ||
            localPos.Y < 0 || localPos.Y >= Chunk.SizeY ||
            localPos.Z < 0 || localPos.Z >= Chunk.SizeZ)
            return;

        chunk.SetBlock(localPos, block);
    }

	private static Vector3i GetChunkPos(Vector3i worldPos)
	{
		return new Vector3i(
			(int)Math.Floor((float)worldPos.X / Chunk.SizeX),
			0,
			(int)Math.Floor((float)worldPos.Z / Chunk.SizeZ)
		);
	}

    public void CheckAndGenerateNewChunk(Vector3 position)
    {
        Vector3i cameraChunkPosition = MathUtils.FloorVector(position / Chunk.ChunkSize);

        if (_oldCameraPosition == cameraChunkPosition)
            return;
        _oldCameraPosition = cameraChunkPosition;

		for (int x = -RenderDistance; x <= RenderDistance; x++)
			for (int z = -RenderDistance; z <= RenderDistance; z++)
				{
					Vector3i chunkPosition = cameraChunkPosition + new Vector3i(x, 0, z);
					if (!Chunks.ContainsKey(chunkPosition))
						GenerateChunk(chunkPosition);
				}
    }

	public bool IsOutsideRenderDistance(Vector3i chunkPos, Vector3 cameraPosition)
	{
		Vector3i cameraChunkPos = MathUtils.FloorVector(cameraPosition / Chunk.ChunkSize);
		int dx = Math.Abs(chunkPos.X - cameraChunkPos.X);
		int dz = Math.Abs(chunkPos.Z - cameraChunkPos.Z);
		return dx > RenderDistance || dz > RenderDistance;
	}

    public Vector3i? IntersectLine(Vector3 start, Vector3 end, ref Vector3i prevBlock)
    {
        Vector3 dir = (end - start).Normalized();
        int dist = (int)Math.Floor(Vector3.Distance(start, end) * 50);

        for (int i = 0; i < dist; i++)
        {
            prevBlock = MathUtils.FloorVector(start);
            start += dir / 50;
            Vector3i blockPos = MathUtils.FloorVector(start);
            BlockType type = GetBlock(blockPos);

            if (type != BlockType.Air)
                return blockPos;
        }

        return null;
    }

    public void Dispose()
    {
        _chunkGenerator.Dispose();
    }
}
