using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace Minecraft.Graphics;

public class Texture
{
	public int TextureId { get; private set; }
	
	public void Create(byte[] data, int width, int height)
	{
		TextureId = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2d, TextureId);
		GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
		GL.GenerateMipmap(TextureTarget.Texture2d);
		
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);	
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
	}

	public void Use()
	{
		GL.BindTexture(TextureTarget.Texture2d, TextureId);
	}

	public static void UnBind()
	{
		GL.BindTexture(TextureTarget.Texture2d, 0);
	}

	public void Delete()
	{
		GL.DeleteTexture(TextureId);
	}
}
