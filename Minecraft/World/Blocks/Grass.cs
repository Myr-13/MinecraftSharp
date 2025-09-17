namespace Minecraft.World.Blocks;

public class Grass : Block
{
	public Grass()
	{
		Type = BlockType.Grass;
		for (int i = (int)Minecraft.Facing.Right; i <= (int)Minecraft.Facing.Back; i++)
			Facing[i] = TextureType.GrassSide;
		Facing[(int)Minecraft.Facing.Up] = TextureType.GrassTop;
		Facing[(int)Minecraft.Facing.Down] = TextureType.Dirt;
	}
}
