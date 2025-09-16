using Minecraft.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Minecraft;

class Program
{
	static void Main()
	{
		// Random r = new(1);
		// bool[,] a = new bool[32, 32];
		// for (int x = 0; x < 32; x++)
		// for (int y = 0; y < 32; y++)
		// 	a[x, y] = r.Next(2) == 1;
		// 	// a[x, y] = Math.Sin(y / 4.0) > 0.5;
		// GreedyMeshing meshing = new(a, 32, 32);
		
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
