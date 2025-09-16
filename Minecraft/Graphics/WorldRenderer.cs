using Minecraft.World;
using OpenTK.Mathematics;

namespace Minecraft.Graphics;

public class WorldRenderer
{
	private List<Mesh> _meshes = [];
	public long VerticesCount { get; private set; }

	public void RebuildMesh(World.World world)
	{
		_meshes.Clear();
		VerticesCount = 0;

		foreach (var entry in world.Chunks)
		{
			Vector3i chunkPosition = entry.Key;
			List<float> vertices = [];

			for (int x = 0; x < Chunk.SizeX; x++)
			{
				for (int y = 0; y < Chunk.SizeY; y++)
				{
					for (int z = 0; z < Chunk.SizeZ; z++)
					{
						Vector3i globalPosition = chunkPosition * Chunk.ChunkSize + new Vector3i(x, y, z);
						Block? currentBlock = world.GetBlock(globalPosition);
						if (currentBlock == null || currentBlock.Type == BlockType.Air)
							continue;

						// -X
						Vector3i neighborPosition = globalPosition + new Vector3i(-1, 0, 0);
						Block? neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							AddQuad(ref vertices, globalPosition, Facing.Left);
						
						// +X
						neighborPosition = globalPosition + new Vector3i(1, 0, 0);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							AddQuad(ref vertices, globalPosition, Facing.Right);
						
						// -Z
						neighborPosition = globalPosition + new Vector3i(0, 0, -1);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							AddQuad(ref vertices, globalPosition, Facing.Back);
						
						// +Z
						neighborPosition = globalPosition + new Vector3i(0, 0, 1);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							AddQuad(ref vertices, globalPosition, Facing.Front);
						
						// -Y
						neighborPosition = globalPosition + new Vector3i(0, -1, 0);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							AddQuad(ref vertices, globalPosition, Facing.Down);
						
						// +Y
						neighborPosition = globalPosition + new Vector3i(0, 1, 0);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							AddQuad(ref vertices, globalPosition, Facing.Up);
					}
				}
			}

			if (vertices.Count > 0)
			{
				Mesh mesh = new Mesh();
				mesh.Create(vertices.ToArray());
				_meshes.Add(mesh);
				VerticesCount += vertices.Count / 8;
			}
		}
	}

	private void AddQuad(ref List<float> vertices, Vector3 position, Facing facing)
	{
		switch (facing)
		{
			// -X
			case Facing.Left:
			{
				vertices.AddRange([
					position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f, 0.0f, 1.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f, 1.0f, 1.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f, 1.0f, 0.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f, 0.0f, 1.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f, 1.0f, 0.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f, 0.0f, 0.0f, 0xFFFFFFFF
				]);

				break;
			}

			// +X
			case Facing.Right:
			{
				vertices.AddRange([
					position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f, 0.0f, 0.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y - 0.5f, position.Z - 0.5f, 1.0f, 1.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y + 0.5f, position.Z - 0.5f, 1.0f, 0.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y - 0.5f, position.Z - 0.5f, 1.0f, 1.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f, 0.0f, 0.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y - 0.5f, position.Z + 0.5f, 0.0f, 1.0f, 0xFFFFFFFF
				]);

				break;
			}

			// -Z
			case Facing.Back:
			{
				vertices.AddRange([
					position.X + 0.5f, position.Y + 0.5f, position.Z - 0.5f, 0.0f, 0.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f, 1.0f, 1.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f, 1.0f, 0.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y + 0.5f, position.Z - 0.5f, 0.0f, 0.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y - 0.5f, position.Z - 0.5f, 0.0f, 1.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f, 1.0f, 1.0f, 0xFFFFFFFF
				]);

				break;
			}

			// +Z
			case Facing.Front:
			{
				vertices.AddRange([
					position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f, 0.0f, 0.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f, 0.0f, 1.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y - 0.5f, position.Z + 0.5f, 1.0f, 1.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f, 1.0f, 0.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f, 0.0f, 0.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y - 0.5f, position.Z + 0.5f, 1.0f, 1.0f, 0xFFFFFFFF
				]);

				break;
			}

			// -Y
			case Facing.Down:
			{
				vertices.AddRange([
					position.X + 0.5f, position.Y - 0.5f, position.Z + 0.5f, 0.0f, 0.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f, 1.0f, 1.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y - 0.5f, position.Z - 0.5f, 1.0f, 0.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y - 0.5f, position.Z + 0.5f, 0.0f, 0.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f, 0.0f, 1.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f, 1.0f, 1.0f, 0xFFFFFFFF
				]);

				break;
			}

			// +Y
			case Facing.Up:
			{
				vertices.AddRange([
					position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f, 0.0f, 1.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y + 0.5f, position.Z - 0.5f, 1.0f, 1.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f, 1.0f, 0.0f, 0xFFFFFFFF,
					position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f, 0.0f, 1.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f, 1.0f, 0.0f, 0xFFFFFFFF,
					position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f, 0.0f, 0.0f, 0xFFFFFFFF
				]);

				break;
			}
		}
	}

	public void Render()
	{
		foreach (var mesh in _meshes)
		{
			mesh.Render();
		}
	}
}
