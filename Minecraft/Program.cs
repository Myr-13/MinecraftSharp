using Minecraft.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

namespace Minecraft;

class Program
{
	static void Main()
	{
		// BinaryGreedyMeshing gm = new();
		// uint[] data = new uint[4];
		// data[0] = 0b0011; // TL - 03, TR - 00
		// data[1] = 0b0111;
		// data[2] = 0b0001;
		// data[3] = 0b1100; // BL - 33, BR - 30
		// gm.ProcessFace(data, 4);

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
