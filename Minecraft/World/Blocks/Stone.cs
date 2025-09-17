namespace Minecraft.World.Blocks;

public class Stone : Block
{
	public Stone()
	{
		Type = BlockType.Stone;
		for (int i = 0; i < Facing.Length; i++)
			Facing[i] = TextureType.Stone;
	}
}
