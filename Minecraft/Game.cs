using ImGuiNET;
using Minecraft.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minecraft;

public class Game
{
    private Window _window;
    private Camera _camera = new(1200, 720);

    private Shader _shader = new();
    private Texture _texture = new();
    private World.World _world = new();
    private WorldRenderer _worldRenderer = new();
    
    private bool _wireframe = false;
    private bool _faceCulling = true;
    private long _lastFpsUpdate;
    private long _lastFps;

    public Game(Window window)
    {
        _window = window;
    }
    
    public void Run()
    {
        _shader.Create("data/shaders/vert.vert", "data/shaders/frag.frag");
        _texture.Create("data/textures/minecraft.png");
        _world.Generate();
        _worldRenderer.RebuildMesh(_world);
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
            _camera.Move(_camera.Up * 1000.0f * deltaTime);
        if (ImGui.IsKeyDown(ImGuiKey.LeftShift))
            _camera.Move(-_camera.Up * 1000.0f * deltaTime);
        
        if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
            _camera.ProcessMouseMovement(0.0f, 2000.0f * deltaTime);
        if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
            _camera.ProcessMouseMovement(0.0f, -2000.0f * deltaTime);
        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
            _camera.ProcessMouseMovement(2000.0f * deltaTime, 0.0f);
        if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
            _camera.ProcessMouseMovement(-2000.0f * deltaTime, 0.0f);
    }

    private void RenderUi()
    {
        // Game Ui
        ImGui.Begin("Game");
        ImGui.Text($"Camera: {_camera.Position.X:F2} {_camera.Position.Y:F2} {_camera.Position.Z:F2}");
        ImGui.Text($"Rotation: {_camera.Yaw % 360.0:F2} {_camera.Pitch % 360.0:F2}");
        ImGui.InputFloat("Camera speed", ref _camera.MovementSpeed);
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
