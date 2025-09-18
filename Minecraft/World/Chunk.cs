using Minecraft.World.Blocks;
using OpenTK.Mathematics;
using t4ccer.Noisy;

namespace Minecraft.World;

public class Chunk
{
	public const int SizeX = 8;
	public const int SizeY = 8;
	public const int SizeZ = 8;
	public static readonly Vector3i ChunkSize = new(SizeX, SizeY, SizeZ);

	private Block?[] _blocks = new Block[SizeX * SizeY * SizeZ];

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
					_blocks[x + y * SizeX + z * SizeX * SizeY] = new Stone();
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
					_blocks[x + y * SizeX + z * SizeX * SizeY] = new Stone();
				}
			}
		}
	}

	public void Generate(Vector3i position)
	{
		GenerateNoise(position);
		// GenerateFill(position);
	}

	public Block? GetBlock(Vector3i position)
	{
		// x + (y * WIDTH) + (z * WIDTH * HEIGHT)
		return GetBlock(position.X, position.Y, position.Z);
	}
	
	public Block? GetBlock(int x, int y, int z)
	{
		// x + (y * WIDTH) + (z * WIDTH * HEIGHT)
		return _blocks[x + y * SizeX + z * SizeX * SizeY];
	}

	public static bool IsInBounds(Vector3i position)
	{
		return position.X >= 0 && position.X < SizeX &&
		       position.Y >= 0 && position.Y < SizeY &&
		       position.Z >= 0 && position.Z < SizeZ;
	}
}
