namespace Minecraft.Graphics;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class Shader
{
	private int _shaderProgram;
	private Dictionary<string, int> _uniforms = new();
	
	public void Create(string vertShaderFile, string fragShaderFile)
	{
		int vertShader = GL.CreateShader(ShaderType.VertexShader);
		GL.ShaderSource(vertShader, File.ReadAllText(vertShaderFile));
        GL.CompileShader(vertShader);
        if (GL.GetShaderi(vertShader, ShaderParameterName.CompileStatus) != 1)
	        throw new Exception($"Failed to compile vertex shader {GL.GetShaderInfoLog(vertShader, 256, out int _)}");
		
        int fragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragShader, File.ReadAllText(fragShaderFile));
        GL.CompileShader(fragShader);
        if (GL.GetShaderi(fragShader, ShaderParameterName.CompileStatus) != 1)
	        throw new Exception($"Failed to compile fragment shader {GL.GetShaderInfoLog(fragShader, 256, out int _)}");
        
        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertShader);
        GL.AttachShader(_shaderProgram, fragShader);
        GL.LinkProgram(_shaderProgram);
		if (GL.GetProgrami(_shaderProgram, ProgramProperty.LinkStatus) != 1)
			throw new Exception($"Failed to link shader program {GL.GetProgramInfoLog(_shaderProgram, 256, out int _)}");
		
		GL.DeleteShader(vertShader);
		GL.DeleteShader(fragShader);
	}

	public void Delete()
	{
		GL.DeleteProgram(_shaderProgram);
	}
	
	public void Use()
	{
		GL.UseProgram(_shaderProgram);
	}

	public void SetUniform(string name, Matrix4 value)
	{
		if (!_uniforms.ContainsKey(name))
			_uniforms[name] = GL.GetUniformLocation(_shaderProgram, name);
		GL.UniformMatrix4f(_uniforms[name], 1, false, ref value);
	}
}
