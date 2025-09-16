using OpenTK.Graphics.OpenGL;

namespace Minecraft.Graphics;

public class Mesh
{
	private int _vbo;
	private int _vao;
	private int _verticesCount;

	public void Create(float[] vertices)
	{
		_vbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsage.StaticDraw);

		_vao = GL.GenVertexArray();
		GL.BindVertexArray(_vao);

		// Позиция (3 float)
		GL.EnableVertexAttribArray(0);
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

		// Текстурные координаты (2 float)
		GL.EnableVertexAttribArray(1);
		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
		
		// Текстурные координаты (1 uint)
		GL.EnableVertexAttribArray(2);
		GL.VertexAttribPointer(2, 1, VertexAttribPointerType.UnsignedInt, false, 6 * sizeof(float), 5 * sizeof(float));

		GL.BindVertexArray(0);

		_verticesCount = vertices.Length / 6;
	}

	public void Render()
	{
		GL.BindVertexArray(_vao);
		GL.DrawArrays(PrimitiveType.Triangles, 0, _verticesCount);
	}
}