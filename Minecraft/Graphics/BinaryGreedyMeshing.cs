using System.Numerics;

namespace Minecraft.Graphics;

public class BinaryGreedyMeshing
{
	public class Quad
	{
		public int X, Y, W, H;
	}
	
	public List<Quad> ProcessFace(uint[] data, int size)
	{
		List<Quad> quads = new();

		for (int row = 0; row < data.Length; row++)
		{
			int y = 0;

			while (y < size)
			{
				y += BitOperations.TrailingZeroCount(data[row] >> y);
				if (y >= size)
					continue;

				int h = BitOperations.TrailingZeroCount(~(data[row] >> y));
				uint hMask = (h >= 32) ? 0xFFFFFFFFu : (1u << h) - 1;
				uint mask = hMask << y;

				int w = 0;
				while (row + w < size)
				{
					uint nextRowH = (data[row + w] >> y) & hMask;
					if (nextRowH != hMask)
						break;
					
					data[row + w] &= ~mask;
					w++;
				}
				
				quads.Add(new Quad { X = row, Y = y, W = w, H = h });

				y += h;
			}
		}

		foreach (var quad in quads)
		{
			Console.WriteLine($"{quad.X} {quad.Y} {quad.W} {quad.H}");
		}

		return quads;
	}
}
