using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Minecraft.World;

namespace Minecraft.Graphics;

public class GreedyMeshing
{
	private readonly List<float> _vertices;
	private readonly Vector3i _chunkBase;
	private readonly Facing _facing;
	private readonly Func<Vector3i, int, int, Vector3i> _rectBasePosMapper;
	private readonly int _dimX;
	private readonly int _dimY;

	public GreedyMeshing(
		bool[,] grid,
		int dimX,
		int dimY,
		List<float> vertices,
		Vector3i chunkBase,
		Facing facing,
		Func<Vector3i, int, int, Vector3i> rectBasePosMapper)
	{
		_vertices = vertices;
		_chunkBase = chunkBase;
		_facing = facing;
		_rectBasePosMapper = rectBasePosMapper;
		_dimX = dimX;
		_dimY = dimY;

		uint[] binaryArray = new uint[dimY];
		bool[,] visited = new bool[dimY, dimX];

		// Заполняем битовые строки
		for (int x = 0; x < dimY; x++)
		{
			uint a = 0;
			for (int y = 0; y < dimX; y++)
			{
				if (grid[x, y])
					a |= 1u << y;
			}

			binaryArray[x] = a;
		}

		// Обрабатываем строки
		for (int x = 0; x < dimY; x++)
		{
			uint a = binaryArray[x];
			int i = 0;

			while (i < dimX)
			{
				if (visited[x, i])
				{
					i++;
					continue;
				}

				if ((a & (1u << i)) != 0)
				{
					int startX = x;
					int startY = i;

					// Ширина
					int width = 0;
					while (i + width < dimX && (a & (1u << (i + width))) != 0 && !visited[x, i + width])
						width++;

					if (width == 0)
					{
						i++;
						continue;
					}

					// Высота
					int height = 1;
					while (startX + height < dimY)
					{
						bool allMatch = true;
						uint nextRow = binaryArray[startX + height];
						for (int w = 0; w < width; w++)
						{
							if ((nextRow & (1u << (i + w))) == 0 || visited[startX + height, i + w])
							{
								allMatch = false;
								break;
							}
						}

						if (!allMatch) break;
						height++;
					}

					// Генерируем прямоугольник
					GenerateQuad(startX, startY, width, height);

					// Помечаем как посещённые
					for (int h = 0; h < height; h++)
					{
						for (int w = 0; w < width; w++)
						{
							visited[startX + h, i + w] = true;
						}
					}

					i += width;
				}
				else
				{
					i++;
				}
			}
		}
	}

	private void GenerateQuad(int gridX, int gridY, int width, int height)
	{
		Vector3 basePos = _rectBasePosMapper(_chunkBase, gridY, gridX);
		AddQuad(_vertices, basePos, _facing, width, height);
	}

	private static void AddQuad(List<float> vertices, Vector3 basePos, Facing facing, int width, int height)
	{
		float x = basePos.X;
		float y = basePos.Y;
		float z = basePos.Z;

		uint color = 0xFFFFFFFF;

		switch (facing)
		{
			case Facing.Left: // -X
				vertices.AddRange([
					x - 0.5f, y - 0.5f, z - 0.5f, 0.0f, 1.0f, color,
					x - 0.5f, y - 0.5f, z + width - 0.5f, 1.0f, 1.0f, color,
					x - 0.5f, y + height - 0.5f, z + width - 0.5f, 1.0f, 0.0f, color,

					x - 0.5f, y - 0.5f, z - 0.5f, 0.0f, 1.0f, color,
					x - 0.5f, y + height - 0.5f, z + width - 0.5f, 1.0f, 0.0f, color,
					x - 0.5f, y + height - 0.5f, z - 0.5f, 0.0f, 0.0f, color
				]);
				break;

			case Facing.Right: // +X
				vertices.AddRange([
					x + 0.5f, y + height - 0.5f, z + width - 0.5f, 0.0f, 0.0f, color,
					x + 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, color,
					x + 0.5f, y + height - 0.5f, z - 0.5f, 1.0f, 0.0f, color,

					x + 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, color,
					x + 0.5f, y + height - 0.5f, z + width - 0.5f, 0.0f, 0.0f, color,
					x + 0.5f, y - 0.5f, z + width - 0.5f, 0.0f, 1.0f, color
				]);
				break;

			case Facing.Back: // -Z
				vertices.AddRange([
					x + width - 0.5f, y + height - 0.5f, z - 0.5f, 0.0f, 0.0f, color,
					x - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, color,
					x - 0.5f, y + height - 0.5f, z - 0.5f, 1.0f, 0.0f, color,

					x + width - 0.5f, y + height - 0.5f, z - 0.5f, 0.0f, 0.0f, color,
					x + width - 0.5f, y - 0.5f, z - 0.5f, 0.0f, 1.0f, color,
					x - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, color
				]);
				break;

			case Facing.Front: // +Z
				vertices.AddRange([
					x - 0.5f, y + height - 0.5f, z + 0.5f, 0.0f, 0.0f, color,
					x - 0.5f, y - 0.5f, z + 0.5f, 0.0f, 1.0f, color,
					x + width - 0.5f, y - 0.5f, z + 0.5f, 1.0f, 1.0f, color,

					x + width - 0.5f, y + height - 0.5f, z + 0.5f, 1.0f, 0.0f, color,
					x - 0.5f, y + height - 0.5f, z + 0.5f, 0.0f, 0.0f, color,
					x + width - 0.5f, y - 0.5f, z + 0.5f, 1.0f, 1.0f, color
				]);
				break;

			case Facing.Down: // -Y
				vertices.AddRange([
					x + width - 0.5f, y - 0.5f, z + width - 0.5f, 0.0f, 0.0f, color,
					x - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, color,
					x + width - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 0.0f, color,

					x + width - 0.5f, y - 0.5f, z + width - 0.5f, 0.0f, 0.0f, color,
					x - 0.5f, y - 0.5f, z + width - 0.5f, 0.0f, 1.0f, color,
					x - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, color
				]);
				break;

			case Facing.Up: // +Y
				vertices.AddRange([
					x + width - 0.5f, y + 0.5f, z + width - 0.5f, 0.0f, 1.0f, color,
					x + width - 0.5f, y + 0.5f, z - 0.5f, 1.0f, 1.0f, color,
					x - 0.5f, y + 0.5f, z - 0.5f, 1.0f, 0.0f, color,

					x + width - 0.5f, y + 0.5f, z + width - 0.5f, 0.0f, 1.0f, color,
					x - 0.5f, y + 0.5f, z - 0.5f, 1.0f, 0.0f, color,
					x - 0.5f, y + 0.5f, z + width - 0.5f, 0.0f, 0.0f, color
				]);
				break;
		}
	}
}