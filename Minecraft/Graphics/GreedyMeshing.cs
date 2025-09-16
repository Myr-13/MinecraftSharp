using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Minecraft.Graphics;

public class GreedyMeshing
{
    private readonly List<float> _vertices;
    private readonly Vector3i _basePosition;
    private readonly Vector3i _faceDirection;
    private readonly int _dimX;
    private readonly int _dimY;

    public GreedyMeshing(bool[,] grid, int dimX, int dimY, List<float> vertices, Vector3i basePosition, Vector3i faceDirection)
    {
        _vertices = vertices;
        _basePosition = basePosition;
        _faceDirection = faceDirection;
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

        // Проходим по сетке и генерируем прямоугольники
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

                    // Генерируем вершины для прямоугольника
                    GenerateQuadVertices(startX, startY, width, height);

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

    private void GenerateQuadVertices(int gridX, int gridY, int width, int height)
    {
        // Определяем локальную позицию прямоугольника в пространстве
        Vector3 startPos = _basePosition + new Vector3(gridY, gridX, 0); // gridY → X, gridX → Y в плоскости грани

        // Для каждой грани — разная ориентация
        switch (_faceDirection)
        {
            case (0, 1, 0): // +Y (верх)
                AddQuad(_vertices, startPos, _faceDirection, width, height);
                break;

            case (0, -1, 0): // -Y (низ)
                AddQuad(_vertices, startPos, _faceDirection, width, height);
                break;

            case (1, 0, 0): // +X
                AddQuad(_vertices, startPos, _faceDirection, width, height);
                break;

            case (-1, 0, 0): // -X
                AddQuad(_vertices, startPos, _faceDirection, width, height);
                break;

            case (0, 0, 1): // +Z
                AddQuad(_vertices, startPos, _faceDirection, width, height);
                break;

            case (0, 0, -1): // -Z
                AddQuad(_vertices, startPos, _faceDirection, width, height);
                break;
        }
    }

    private static void AddQuad(List<float> vertices, Vector3 basePos, Vector3i direction, int width, int height)
    {
        float x = basePos.X;
        float y = basePos.Y;
        float z = basePos.Z;

        float w = width;
        float h = height;

        switch (direction)
        {
            case (-1, 0, 0): // -X
                vertices.AddRange([
                    x - 0.5f, y - 0.5f, z - 0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y - 0.5f, z + w - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y + h - 0.5f, z + w - 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,

                    x - 0.5f, y - 0.5f, z - 0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y + h - 0.5f, z + w - 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y + h - 0.5f, z - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f
                ]);
                break;

            case (1, 0, 0): // +X
                vertices.AddRange([
                    x + w - 0.5f, y + h - 0.5f, z + w - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x + w - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x + w - 0.5f, y + h - 0.5f, z - 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,

                    x + w - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x + w - 0.5f, y + h - 0.5f, z + w - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x + w - 0.5f, y - 0.5f, z + w - 0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f
                ]);
                break;

            case (0, 0, -1): // -Z
                vertices.AddRange([
                    x + w - 0.5f, y + h - 0.5f, z - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y + h - 0.5f, z - 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,

                    x + w - 0.5f, y + h - 0.5f, z - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x + w - 0.5f, y - 0.5f, z - 0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f
                ]);
                break;

            case (0, 0, 1): // +Z
                vertices.AddRange([
                    x - 0.5f, y + h - 0.5f, z + w - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y - 0.5f, z + w - 0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x + w - 0.5f, y - 0.5f, z + w - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,

                    x + w - 0.5f, y + h - 0.5f, z + w - 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y + h - 0.5f, z + w - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x + w - 0.5f, y - 0.5f, z + w - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f
                ]);
                break;

            case (0, -1, 0): // -Y
                vertices.AddRange([
                    x + w - 0.5f, y - 0.5f, z + w - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x + w - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,

                    x + w - 0.5f, y - 0.5f, z + w - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y - 0.5f, z + w - 0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y - 0.5f, z - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f
                ]);
                break;

            case (0, 1, 0): // +Y
                vertices.AddRange([
                    x + w - 0.5f, y + h - 0.5f, z + w - 0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x + w - 0.5f, y + h - 0.5f, z - 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y + h - 0.5f, z - 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,

                    x + w - 0.5f, y + h - 0.5f, z + w - 0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y + h - 0.5f, z - 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                    x - 0.5f, y + h - 0.5f, z + w - 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f
                ]);
                break;
        }
    }
}
