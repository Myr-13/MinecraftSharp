using Minecraft.World.Blocks;
using OpenTK.Mathematics;
using t4ccer.Noisy;

namespace Minecraft.World;

public class Chunk
{
	public const int SizeX = 32;
	public const int SizeY = 32;
	public const int SizeZ = 32;
	public static readonly Vector3i ChunkSize = new(SizeX, SizeY, SizeZ);

	private BlockType[] _blocks = new BlockType[SizeX * SizeY * SizeZ];

	private void GenerateNoise(Vector3i position)
	{
		var noise = new OpenSimplexNoise3DGenerator(1);
		
		for (int x = 0; x < SizeX; x++)
		{
			for (int y = 0; y < SizeY; y++)
			{
				for (int z = 0; z < SizeZ; z++)
				{
					float noiseX = position.X * SizeX + x;
					float noiseY = position.Y * SizeY + y;
					float noiseZ = position.Z * SizeZ + z;
					if (noise.At(noiseX / SizeX, noiseY / SizeY, noiseZ / SizeZ) < 0.3f)
						continue;
					_blocks[x + y * SizeX + z * SizeX * SizeY] = BlockType.Gravel;
				}
			}
		}
	}

	private void GenerateFill(Vector3i position)
	{
		for (int x = 0; x < SizeX; x++)
		{
			for (int y = 0; y < SizeY; y++)
			{
				for (int z = 0; z < SizeZ; z++)
				{
					_blocks[x + y * SizeX + z * SizeX * SizeY] = BlockType.Gravel;
				}
			}
		}
	}

	public void Generate(Vector3i position)
	{
		// GenerateNoise(position);
		GenerateFill(position);
	}

	public BlockType GetBlock(Vector3i position)
	{
		return GetBlock(position.X, position.Y, position.Z);
	}
	
	public BlockType GetBlock(int x, int y, int z)
	{
		// x + (y * WIDTH) + (z * WIDTH * HEIGHT)
		return _blocks[x + y * SizeX + z * SizeX * SizeY];
	}

	public void SetBlock(Vector3i position, BlockType block)
	{
		SetBlock(position.X, position.Y, position.Z, block);
	}

	public void SetBlock(int x, int y, int z, BlockType block)
	{
		_blocks[x + y * SizeX + z * SizeX * SizeY] = block;
	}

	public static bool IsInBounds(Vector3i position)
	{
		return position.X >= 0 && position.X < SizeX &&
		       position.Y >= 0 && position.Y < SizeY &&
		       position.Z >= 0 && position.Z < SizeZ;
	}
}
