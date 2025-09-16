using OpenTK.Mathematics;

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
		Random random = new Random(1 + position.X * 100 + position.Y * 10 + position.Z);
		
		for (int i = 0; i < SizeX * SizeY * SizeZ; i++)
		{
			Blocks[i] = new()
			{
				// Type = (BlockType)random.Next(0, (int)BlockType.Count)
				Type = BlockType.Stone
			};
		}
	}

	public Block GetBlock(Vector3i position)
	{
		// x + (y * WIDTH) + (z * WIDTH * HEIGHT)
		return Blocks[position.X + (position.Y + SizeX) + (position.Z * SizeX * SizeY)];
	}

	public static bool IsInBounds(Vector3i position)
	{
		return position.X >= 0 && position.X < SizeX &&
		       position.Y >= 0 && position.Y < SizeY &&
		       position.Z >= 0 && position.Z < SizeZ;
	}
}
