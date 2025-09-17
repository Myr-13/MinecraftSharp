using Minecraft.World.Blocks;
using OpenTK.Mathematics;
using t4ccer.Noisy;

namespace Minecraft.World;

public class Chunk
{
	public const int SizeX = 4;
	public const int SizeY = 4;
	public const int SizeZ = 4;
	public static readonly Vector3i ChunkSize = new(SizeX, SizeY, SizeZ);
	
	public Block[] Blocks = new Block[SizeX * SizeY * SizeZ];

	public void Generate(Vector3i position)
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
					Blocks[x + y * SizeX + z * SizeX * SizeY] = new Stone();
				}
			}
		}
	}

	public Block GetBlock(Vector3i position)
	{
		// x + (y * WIDTH) + (z * WIDTH * HEIGHT)
		return Blocks[position.X + position.Y * SizeX + position.Z * SizeX * SizeY];
	}

	public static bool IsInBounds(Vector3i position)
	{
		return position.X >= 0 && position.X < SizeX &&
		       position.Y >= 0 && position.Y < SizeY &&
		       position.Z >= 0 && position.Z < SizeZ;
	}
}
