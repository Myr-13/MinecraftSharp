using StbImageSharp;

namespace Minecraft.Graphics;

public static class TextureContainer
{
	private static List<ImageResult> atlases = new();
	private static List<Texture> textures = new();
	
	public static int AddTextureAtlas(string path)
	{
		byte[] content = File.ReadAllBytes(path);
		ImageResult image = ImageResult.FromMemory(content);
		
		if (image.SourceComp != ColorComponents.RedGreenBlueAlpha)
			throw new InvalidOperationException($"Image at {path} must have 4 components (RGBA), but has {image.SourceComp}");
		
		atlases.Add(image);
		return atlases.Count - 1;
	}

	public static int LoadTextureFromAtlas(int atlasId, int x, int y, int width, int height)
	{
		if (atlasId < 0 || atlasId >= atlases.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(atlasId), $"Atlas with ID {atlasId} does not exist");
		}

		ImageResult atlas = atlases[atlasId];
        
		if (x < 0 || y < 0 || 
		    x + width > atlas.Width || 
		    y + height > atlas.Height)
		{
			throw new ArgumentException($"Requested region (x:{x}, y:{y}, w:{width}, h:{height}) " +
			                            $"is out of bounds for atlas (w:{atlas.Width}, h:{atlas.Height})");
		}

		byte[] subTextureData = new byte[width * height * 4];
        
		for (int row = 0; row < height; row++)
		{
			int srcRow = y + row;
			int srcOffset = srcRow * atlas.Width * 4 + x * 4;
			int dstOffset = row * width * 4;
            
			Array.Copy(atlas.Data, srcOffset, subTextureData, dstOffset, width * 4);
		}

		Texture texture = new();
		texture.Create(subTextureData, width, height);
		textures.Add(texture);
		return textures.Count - 1;
	}

	public static Texture GetTexture(int textureId)
	{
		return textures[textureId];
	}

	public static void Shutdown()
	{
		foreach (Texture texture in textures)
			texture.Delete();
	}
}
