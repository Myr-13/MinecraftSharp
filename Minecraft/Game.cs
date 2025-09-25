using System.Runtime.InteropServices;
using ImGuiNET;
using Minecraft.Graphics;
using Minecraft.World;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using NVector2 = System.Numerics.Vector2;
using NVector4 = System.Numerics.Vector4;

namespace Minecraft;

public class Game
{
	private Window _window;
	private Camera _camera = new(1200, 720);

	private Shader _shader = new();
	private WorldRenderer _worldRenderer = new();
	private World.World _world;

	private BlockType _selectedBlockType = BlockType.Stone;
	private int _textureAtlasId;

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
		int textureAtlasId = TextureContainer.AddTextureAtlas("data/textures/minecraft.png");

		TextureContainer.LoadTextureFromAtlas(textureAtlasId, 176, 16, 16, 16); // Air
		TextureContainer.LoadTextureFromAtlas(textureAtlasId, 0, 0, 16, 16); // Stone
		TextureContainer.LoadTextureFromAtlas(textureAtlasId, 16, 0, 16, 16); // Dirt
		TextureContainer.LoadTextureFromAtlas(textureAtlasId, 32, 0, 16, 16); // Grass
		TextureContainer.LoadTextureFromAtlas(textureAtlasId, 48, 16, 16, 16); // Gravel
		_textureAtlasId = TextureContainer.LoadTextureFromAtlas(textureAtlasId, 0, 0, 256, 256);
		
		_shader.Create("data/shaders/block.vert", "data/shaders/block.frag");
		_world.CheckAndGenerateNewChunk(Vector3.One);

		//_camera.OnCameraMovement += () => _world.CheckAndGenerateNewChunk(_camera.Position);
		_world.GenerateChunk(Vector3i.Zero);
	}

	public void Shutdown()
	{
		_shader.Delete();
	}

	public void OnRender()
	{
		RenderUi();
		
		TextureContainer.GetTexture(_textureAtlasId).Use();
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

		if (ImGui.IsKeyDown(ImGuiKey._1))
			_selectedBlockType = BlockType.Stone;
		if (ImGui.IsKeyDown(ImGuiKey._2))
			_selectedBlockType = BlockType.Dirt;
		if (ImGui.IsKeyDown(ImGuiKey._3))
			_selectedBlockType = BlockType.Grass;
		if (ImGui.IsKeyDown(ImGuiKey._4))
			_selectedBlockType = BlockType.Gravel;

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
				_world.SetBlock(prevBlock, _selectedBlockType);
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
		
		// Graphics Ui
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
		
		// Hotbar Ui
		{
			ImDrawListPtr backgroundPtr = ImGui.GetBackgroundDrawList();
			Vector2 windowSize = _window.Size;
			NVector2 position = new NVector2(windowSize.X / 2, windowSize.Y - 64);

			uint selectedColor = ImGui.ColorConvertFloat4ToU32(new NVector4(1.0f, 1.0f, 1.0f, 1.0f));
			uint unselectedColor = ImGui.ColorConvertFloat4ToU32(new NVector4(0.2f, 0.2f, 0.2f, 1.0f));

			for (int i = 1; i < (int)BlockType.Count; i++)
			{
				backgroundPtr.AddRect(
					position - new NVector2(18, 18),
					position + new NVector2(18, 18),
					(int)_selectedBlockType == i ? selectedColor : unselectedColor,
					0.0f, ImDrawFlags.None, 3.0f);
				backgroundPtr.AddImage(TextureContainer.GetTexture(i).TextureId, position - new NVector2(16, 16), position + new NVector2(16, 16));
				position.X += 48;
			}
		}
	}
}
