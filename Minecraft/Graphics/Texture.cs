using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace Minecraft.Graphics;

public class Texture
{
	private int _texture;
	
	public void Create(string texturePath)
	{
		// StbImage.stbi_set_flip_vertically_on_load(1);

		byte[] content = File.ReadAllBytes(texturePath);
		ImageResult image = ImageResult.FromMemory(content);

		_texture = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2d, _texture);
		GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
		GL.GenerateMipmap(TextureTarget.Texture2d);
		
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);	
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
	}

	public void Use()
	{
		GL.BindTexture(TextureTarget.Texture2d, _texture);
	}

	public static void UnBind()
	{
		GL.BindTexture(TextureTarget.Texture2d, 0);
	}

	public void Delete()
	{
		GL.DeleteTexture(_texture);
	}
}
