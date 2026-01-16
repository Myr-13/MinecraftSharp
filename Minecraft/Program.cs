using OpenTK.Platform;

namespace Minecraft;

class Program
{
	static void Main()
	{
		ToolkitOptions options = new()
		{
			ApplicationName = "Minecraft"
		};
		Toolkit.Init(options);
		
		Window wnd = new Window();
		wnd.Run();
		wnd.OnClosed();
	}
}
