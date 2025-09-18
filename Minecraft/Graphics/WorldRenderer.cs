using Minecraft.World;
using Minecraft.World.Blocks;
using OpenTK.Mathematics;

namespace Minecraft.Graphics;

public class WorldRenderer
{
	private List<Mesh> _meshes = [];
	public long VerticesCount { get; private set; }

	private struct Face
	{
		public required Facing Facing;
		public required Block Block;
	}

	public void RebuildMesh(World.World world)
	{
		_meshes.Clear();
		VerticesCount = 0;

		foreach (var entry in world.Chunks)
		{
			Vector3i chunkPosition = entry.Key;
			List<float> vertices = [];
			Dictionary<Vector3i, List<Face>> facesList = new();

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

						List<Face> faces = [];

						// -X
						Vector3i neighborPosition = globalPosition + new Vector3i(-1, 0, 0);
						Block? neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							faces.Add(new Face { Facing = Facing.Left, Block = currentBlock });
						
						// +X
						neighborPosition = globalPosition + new Vector3i(1, 0, 0);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							faces.Add(new Face { Facing = Facing.Right, Block = currentBlock });
						
						// -Z
						neighborPosition = globalPosition + new Vector3i(0, 0, -1);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							faces.Add(new Face { Facing = Facing.Back, Block = currentBlock });
						
						// +Z
						neighborPosition = globalPosition + new Vector3i(0, 0, 1);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							faces.Add(new Face { Facing = Facing.Front, Block = currentBlock });
						
						// -Y
						neighborPosition = globalPosition + new Vector3i(0, -1, 0);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							faces.Add(new Face { Facing = Facing.Down, Block = currentBlock });
						
						// +Y
						neighborPosition = globalPosition + new Vector3i(0, 1, 0);
						neighbor = world.GetBlock(neighborPosition);
						if (neighbor == null || neighbor.Type == BlockType.Air)
							faces.Add(new Face { Facing = Facing.Up, Block = currentBlock });

						facesList[globalPosition] = faces;
					}
				}
			}

			foreach (var faceEntry in facesList)
			{
				Vector3i facePosition = faceEntry.Key;
				
				foreach (Face face in faceEntry.Value)
				{
					if (face.Facing == Facing.Up)
					{
						Vector3i next = facePosition + new Vector3i(1, 0, 0);
						int w = 1;
						int h = 1;

						// Grow by width
						while (facesList.Remove(next))
						{
							next.X += 1;
							w++;
						}

						// Grow by height
						while (true)
						{
							bool brk = false;
							for (int i = 0; i < w; i++)
							{
								next = facePosition + new Vector3i(i, 0, h);
								if (!facesList.ContainsKey(next))
								{
									brk = true;
									break;
								}
							}
							
							if (brk)
								break;
							
							h++;
							
							if (h >= Chunk.SizeZ)
								break;
						}

						for (int i = 0; i < h; i++)
						{
							for (int j = 0; j < w; j++)
							{
								facesList.Remove(facePosition + new Vector3i(j, 0, i));
							}
						}
						
						AddQuad(ref vertices, facePosition, new Vector2i(w, h), face.Facing, face.Block);
					}
				}
			}

			if (vertices.Count > 0)
			{
				Mesh mesh = new Mesh();
				mesh.Create(vertices.ToArray());
				_meshes.Add(mesh);
				VerticesCount += mesh.VerticesCount;
			}
		}
	}

	private void AddQuad(ref List<float> vertices, Vector3 position, Vector2i size, Facing facing, Block block)
	{
		int faceTexture = (int)block.Facing[(int)facing];
		
		switch (facing)
		{
			// -X
			case Facing.Left:
			{
				vertices.AddRange([
					position.X, position.Y,          position.Z,          0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X, position.Y,          position.Z + size.X, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X, position.Y + size.Y, position.Z + size.X, 1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X, position.Y,          position.Z,          0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X, position.Y + size.Y, position.Z + size.X, 1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X, position.Y + size.Y, position.Z,          0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
				]);
				break;
			}

			// +X
			case Facing.Right:
			{
				vertices.AddRange([
					position.X + 1.0f, position.Y + size.Y, position.Z + size.X, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + 1.0f, position.Y,          position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + 1.0f, position.Y + size.Y, position.Z,          1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + 1.0f, position.Y,          position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + 1.0f, position.Y + size.Y, position.Z + size.X, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + 1.0f, position.Y,          position.Z + size.X, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
				]);

				break;
			}

			// -Z
			case Facing.Back:
			{
				vertices.AddRange([
					position.X + size.X, position.Y + size.Y, position.Z, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y,          position.Z, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y + size.Y, position.Z, 1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + size.X, position.Y + size.Y, position.Z, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + size.X, position.Y,          position.Z, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y,          position.Z, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
				]);

				break;
			}

			// +Z
			case Facing.Front:
			{
				vertices.AddRange([
					position.X,          position.Y + size.Y, position.Z + 1.0f, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y,          position.Z + 1.0f, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + size.X, position.Y,          position.Z + 1.0f, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + size.X, position.Y + size.Y, position.Z + 1.0f, 1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y + size.Y, position.Z + 1.0f, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + size.X, position.Y,          position.Z + 1.0f, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
				]);

				break;
			}

			// -Y
			case Facing.Down:
			{
				vertices.AddRange([
					position.X + size.Y, position.Y, position.Z + size.X, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y, position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + size.Y, position.Y, position.Z,          1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + size.Y, position.Y, position.Z + size.X, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y, position.Z + size.X, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y, position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
				]);

				break;
			}

			// +Y
			case Facing.Up:
			{
				vertices.AddRange([
					position.X + size.X, position.Y + 1.0f, position.Z + size.Y, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + size.X, position.Y + 1.0f, position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y + 1.0f, position.Z,          1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X + size.X, position.Y + 1.0f, position.Z + size.Y, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y + 1.0f, position.Z,          1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
					position.X,          position.Y + 1.0f, position.Z + size.Y, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f,
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
