using OpenTK.Graphics.OpenGL;

namespace Minecraft.Graphics;

public class Mesh
{
	private int _vbo;
	private int _vao;
	public int VerticesCount { get; private set; }

	public void Create(float[] vertices)
	{
		_vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsage.StaticDraw);

		_vao = GL.GenVertexArray();
		GL.BindVertexArray(_vao);

		// Position (3 float)
		GL.EnableVertexAttribArray(0);
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 0);

		// Texture UV (2 float)
		GL.EnableVertexAttribArray(1);
		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 11 * sizeof(float), 3 * sizeof(float));
		
		// BlockId (1 float)
		GL.EnableVertexAttribArray(2);
		GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 11 * sizeof(float), 5 * sizeof(float));
		
		// Color (3 float)
		GL.EnableVertexAttribArray(3);
		GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 6 * sizeof(float));
		
		// Mesh size (2 float)
		GL.EnableVertexAttribArray(4);
		GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, 11 * sizeof(float), 9 * sizeof(float));

		GL.BindVertexArray(0);

		VerticesCount = vertices.Length / 11;
	}

	public void Delete()
	{
		GL.DeleteBuffer(_vbo);
		GL.DeleteVertexArray(_vao);
	}

	public void Render()
	{
		GL.BindVertexArray(_vao);
		GL.DrawArrays(PrimitiveType.Triangles, 0, VerticesCount);
	}
}