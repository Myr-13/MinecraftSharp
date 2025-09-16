using ImGuiNET;
using Minecraft.Ui.Backends;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Minecraft;

public class Window : GameWindow
{
	private Game _game;
	
	public Window() : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = new Vector2i(1200, 720), APIVersion = new Version(4, 1) })
	{
	}
	
	protected override void OnLoad()
	{
		base.OnLoad();
		
		GL.Enable(EnableCap.CullFace);
		
		// ImGui
		ImGui.CreateContext();

		ImGui.StyleColorsDark();

		ImguiImplOpenTK4.Init(this);
		ImguiImplOpenGL3.Init();

		_game = new(this);
		_game.Run();
	}
	
	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);
		
		_game.OnUpdate();
		
		GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
		GL.ClearColor(new Color4<Rgba>(0, 0, 0.2f, 1.0f));
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		
		ImguiImplOpenGL3.NewFrame();
		ImguiImplOpenTK4.NewFrame();
		ImGui.NewFrame();
		_game.OnRender();
		ImGui.Render();
		
		ImguiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());

		SwapBuffers();
	}

	public void OnClosed()
	{
		_game.Shutdown();
		ImguiImplOpenGL3.Shutdown();
		ImguiImplOpenTK4.Shutdown();
	}
}
