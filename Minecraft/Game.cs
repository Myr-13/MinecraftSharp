using ImGuiNET;
using Minecraft.Graphics;
using Minecraft.World;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Minecraft;

public class Game
{
	private Window _window;
	private Camera _camera = new(1200, 720);

	private Shader _shader = new();
	private Texture _texture = new();
	private WorldRenderer _worldRenderer = new();
	private World.World _world;

	private bool _wireframe = false;
	private bool _faceCulling = true;
	private long _lastFpsUpdate;
	private long _lastFps;

	public Game(Window window)
	{
		_world = new(_worldRenderer);
		_window = window;
	}
	
	public void Run()
	{
		ModelsContainer.LoadModels("data/models");
		
		_shader.Create("data/shaders/vert.vert", "data/shaders/frag.frag");
		_texture.Create("data/textures/minecraft.png");
		_world.CheckAndGenerateNewChunk(Vector3.One);

		//_camera.OnCameraMovement += () => _world.CheckAndGenerateNewChunk(_camera.Position);
		_world.GenerateChunk(Vector3i.Zero);
	}

	public void Shutdown()
	{
		_shader.Delete();
		_texture.Delete();
	}

	public void OnRender()
	{
		RenderUi();
		
		_texture.Use();
		_shader.Use();
		_shader.SetUniform("view", _camera.View);
		_shader.SetUniform("projection", _camera.Projection);
		_worldRenderer.Render();
	}

	public void OnUpdate()
	{
		float deltaTime = (float)_window.UpdateTime;
		
		if (ImGui.IsKeyDown(ImGuiKey.W))
			_camera.Move(_camera.Front * 1000.0f * deltaTime);
		if (ImGui.IsKeyDown(ImGuiKey.S))
			_camera.Move(-_camera.Front * 1000.0f * deltaTime);
		if (ImGui.IsKeyDown(ImGuiKey.A))
			_camera.Move(-_camera.Right * 1000.0f * deltaTime);
		if (ImGui.IsKeyDown(ImGuiKey.D))
			_camera.Move(_camera.Right * 1000.0f * deltaTime);
		if (ImGui.IsKeyDown(ImGuiKey.Space))
			_camera.Move(Vector3.UnitY * 1000.0f * deltaTime);
		if (ImGui.IsKeyDown(ImGuiKey.LeftShift))
			_camera.Move(-Vector3.UnitY * 1000.0f * deltaTime);
		
		if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
			_camera.ProcessMouseMovement(0.0f, 200.0f * deltaTime);
		if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
			_camera.ProcessMouseMovement(0.0f, -200.0f * deltaTime);
		if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
			_camera.ProcessMouseMovement(200.0f * deltaTime, 0.0f);
		if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
			_camera.ProcessMouseMovement(-200.0f * deltaTime, 0.0f);

		if (ImGui.IsKeyPressed(ImGuiKey.E))
		{
			Vector3i prevBlock = new();
			Vector3i? blockPos = _world.IntersectLine(_camera.Position, _camera.Position + _camera.Front * 10, ref prevBlock);
			Console.WriteLine($"Block: {blockPos}");
			
			if (blockPos != null)
				_world.SetBlock(blockPos.Value, BlockType.Air);
		}
		
		if (ImGui.IsKeyPressed(ImGuiKey.R))
		{
			Vector3i prevBlock = new();
			Vector3i? blockPos = _world.IntersectLine(_camera.Position, _camera.Position + _camera.Front * 10, ref prevBlock);
			Console.WriteLine($"Block: {blockPos}");
			
			if (blockPos != null)
				_world.SetBlock(prevBlock, BlockType.Stone);
		}
	}

	private void RenderUi()
	{
		// Game Ui
		ImGui.Begin("Game");
		ImGui.Text($"Camera: {_camera.Position.X:F2} {_camera.Position.Y:F2} {_camera.Position.Z:F2}");
		ImGui.Text($"Rotation: {_camera.Yaw % 360.0:F2} {_camera.Pitch % 360.0:F2}");
		ImGui.InputFloat("Camera speed", ref _camera.MovementSpeed);
		ImGui.Text($"Chunks count: {_world.Chunks.Count}");
		ImGui.End();
		
		// Graphics Ui
		// Update fps every 0.1 second
		if (_lastFpsUpdate + 1_000_000 < DateTime.Now.Ticks)
		{
			_lastFpsUpdate = DateTime.Now.Ticks;
			_lastFps = (int)Math.Round(1.0 / _window.UpdateTime);
		}
		
		long vertCount = _worldRenderer.VerticesCount;
		long bytes = vertCount * 9 * sizeof(float);
		
		ImGui.Begin("Graphics");
		ImGui.Text($"Vertices: {vertCount} Triangles: {vertCount / 3}");
		ImGui.Text($"Allocated: {bytes / 1024.0:F1} KiB");
		ImGui.Text($"FPS: {_lastFps}");
		
		if (ImGui.Checkbox("Wireframe", ref _wireframe))
			GL.PolygonMode(TriangleFace.FrontAndBack, _wireframe ? PolygonMode.Line : PolygonMode.Fill);
			
		if (ImGui.Checkbox("Face culling", ref _faceCulling))
		{
			if (_faceCulling)
				GL.Enable(EnableCap.CullFace);
			else
				GL.Disable(EnableCap.CullFace);
		}
		ImGui.End();
	}
}
