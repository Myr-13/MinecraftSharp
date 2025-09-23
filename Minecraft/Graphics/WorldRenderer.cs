using Minecraft.World;
using Minecraft.World.Blocks;
using OpenTK.Mathematics;

namespace Minecraft.Graphics;

public class WorldRenderer
{
	private Dictionary<Vector3i, Mesh> _meshes = new();
	public long VerticesCount { get; private set; }

	public void RebuildChunkMesh(World.World world, Vector3i chunkPosition)
	{
		if (_meshes.ContainsKey(chunkPosition))
		{
			VerticesCount -= _meshes[chunkPosition].VerticesCount;
			_meshes[chunkPosition].Delete();
			_meshes.Remove(chunkPosition);
		}
		
		List<float> vertices = [];
		Dictionary<Vector3i, Dictionary<Facing, BlockType>> facesList = new();

		for (int x = 0; x < Chunk.SizeX; x++)
		{
			for (int y = 0; y < Chunk.SizeY; y++)
			{
				for (int z = 0; z < Chunk.SizeZ; z++)
				{
					Vector3i globalPosition = chunkPosition * Chunk.ChunkSize + new Vector3i(x, y, z);
					BlockType currentBlockType = world.GetBlock(globalPosition);
					if (currentBlockType == BlockType.Air)
						continue;

					Dictionary<Facing, BlockType> faces = [];

					// -X
					Vector3i neighborPosition = globalPosition + new Vector3i(-1, 0, 0);
					BlockType neighborBlockType = world.GetBlock(neighborPosition);
					if (neighborBlockType == BlockType.Air)
						faces[Facing.Left] = currentBlockType;

					// +X
					neighborPosition = globalPosition + new Vector3i(1, 0, 0);
					neighborBlockType = world.GetBlock(neighborPosition);
					if (neighborBlockType == BlockType.Air)
						faces[Facing.Right] = currentBlockType;

					// -Z
					neighborPosition = globalPosition + new Vector3i(0, 0, -1);
					neighborBlockType = world.GetBlock(neighborPosition);
					if (neighborBlockType == BlockType.Air)
						faces[Facing.Back] = currentBlockType;

					// +Z
					neighborPosition = globalPosition + new Vector3i(0, 0, 1);
					neighborBlockType = world.GetBlock(neighborPosition);
					if (neighborBlockType == BlockType.Air)
						faces[Facing.Front] = currentBlockType;

					// -Y
					neighborPosition = globalPosition + new Vector3i(0, -1, 0);
					neighborBlockType = world.GetBlock(neighborPosition);
					if (neighborBlockType == BlockType.Air)
						faces[Facing.Down] = currentBlockType;

					// +Y
					neighborPosition = globalPosition + new Vector3i(0, 1, 0);
					neighborBlockType = world.GetBlock(neighborPosition);
					if (neighborBlockType == BlockType.Air)
						faces[Facing.Up] = currentBlockType;

					facesList[globalPosition] = faces;
				}
			}
		}

		// Создаем копию для безопасного перебора
		var facesListCopy = new Dictionary<Vector3i, Dictionary<Facing, BlockType>>(facesList);

		foreach (var faceEntry in facesListCopy)
		{
			Vector3i facePosition = faceEntry.Key;

			// Создаем копию для безопасного перебора граней
			var facesCopy = new Dictionary<Facing, BlockType>(faceEntry.Value);

			foreach (var face in facesCopy)
			{
				// Проверяем, не была ли грань уже удалена при объединении
				if (!facesList.TryGetValue(facePosition, out var currentFaces) ||
					!currentFaces.ContainsKey(face.Key))
					continue;

				switch (face.Key)
				{
					case Facing.Up:
					{
						int w = 1;
						int h = 1;

						// Расширение по X (ширина)
						while (w < Chunk.SizeX)
						{
							Vector3i testPos = facePosition + new Vector3i(w, 0, 0);
							if (!facesList.TryGetValue(testPos, out var testFaces) ||
								!testFaces.ContainsKey(Facing.Up) ||
								testFaces[Facing.Up] != face.Value)
								break;
							w++;
						}

						// Расширение по Z (высота)
						bool canGrowHeight;
						do
						{
							canGrowHeight = true;
							for (int i = 0; i < w; i++)
							{
								Vector3i testPos = facePosition + new Vector3i(i, 0, h);
								if (!facesList.TryGetValue(testPos, out var testFaces) ||
									!testFaces.ContainsKey(Facing.Up) ||
									testFaces[Facing.Up] != face.Value)
								{
									canGrowHeight = false;
									break;
								}
							}

							if (canGrowHeight)
								h++;
						} while (canGrowHeight && h < Chunk.SizeZ);

						// Удаляем обработанные грани
						for (int i = 0; i < h; i++)
						{
							for (int j = 0; j < w; j++)
							{
								Vector3i pos = facePosition + new Vector3i(j, 0, i);
								if (facesList.TryGetValue(pos, out var posFaces))
								{
									posFaces.Remove(Facing.Up);
									if (posFaces.Count == 0)
										facesList.Remove(pos);
								}
							}
						}

						AddQuad(ref vertices, facePosition, new Vector2i(w, h), face.Key, face.Value);
						break;
					}

					case Facing.Down:
					{
						int w = 1;
						int h = 1;

						// Расширение по X (ширина)
						while (w < Chunk.SizeX)
						{
							Vector3i testPos = facePosition + new Vector3i(w, 0, 0);
							if (!facesList.TryGetValue(testPos, out var testFaces) ||
								!testFaces.ContainsKey(Facing.Down) ||
								testFaces[Facing.Down] != face.Value)
								break;
							w++;
						}

						// Расширение по Z (высота)
						bool canGrowHeight;
						do
						{
							canGrowHeight = true;
							for (int i = 0; i < w; i++)
							{
								Vector3i testPos = facePosition + new Vector3i(i, 0, h);
								if (!facesList.TryGetValue(testPos, out var testFaces) ||
									!testFaces.ContainsKey(Facing.Down) ||
									testFaces[Facing.Down] != face.Value)
								{
									canGrowHeight = false;
									break;
								}
							}

							if (canGrowHeight)
								h++;
						} while (canGrowHeight && h < Chunk.SizeZ);

						// Удаляем обработанные грани
						for (int i = 0; i < h; i++)
						{
							for (int j = 0; j < w; j++)
							{
								Vector3i pos = facePosition + new Vector3i(j, 0, i);
								if (facesList.TryGetValue(pos, out var posFaces))
								{
									posFaces.Remove(Facing.Down);
									if (posFaces.Count == 0)
										facesList.Remove(pos);
								}
							}
						}

						AddQuad(ref vertices, facePosition, new Vector2i(w, h), face.Key, face.Value);
						break;
					}

					case Facing.Left:
					{
						int w = 1;
						int h = 1;

						// Расширение по Z (ширина) - правильно для левой грани
						while (w < Chunk.SizeZ)
						{
							Vector3i testPos = facePosition + new Vector3i(0, 0, w);
							if (!facesList.TryGetValue(testPos, out var testFaces) ||
								!testFaces.ContainsKey(Facing.Left) ||
								testFaces[Facing.Left] != face.Value)
								break;
							w++;
						}

						// Расширение по Y (высота) - правильно для левой грани
						bool canGrowHeight;
						do
						{
							canGrowHeight = true;
							for (int i = 0; i < w; i++)
							{
								Vector3i testPos = facePosition + new Vector3i(0, h, i);
								if (!facesList.TryGetValue(testPos, out var testFaces) ||
									!testFaces.ContainsKey(Facing.Left) ||
									testFaces[Facing.Left] != face.Value)
								{
									canGrowHeight = false;
									break;
								}
							}

							if (canGrowHeight)
								h++;
						} while (canGrowHeight && h < Chunk.SizeY);

						// Удаляем обработанные грани
						for (int i = 0; i < h; i++)
						{
							for (int j = 0; j < w; j++)
							{
								Vector3i pos = facePosition + new Vector3i(0, i, j);
								if (facesList.TryGetValue(pos, out var posFaces))
								{
									posFaces.Remove(Facing.Left);
									if (posFaces.Count == 0)
										facesList.Remove(pos);
								}
							}
						}

						AddQuad(ref vertices, facePosition, new Vector2i(w, h), face.Key, face.Value);
						break;
					}

					case Facing.Right:
					{
						int w = 1;
						int h = 1;

						// Расширение по Z (ширина) - правильно для правой грани
						while (w < Chunk.SizeZ)
						{
							Vector3i testPos = facePosition + new Vector3i(0, 0, w);
							if (!facesList.TryGetValue(testPos, out var testFaces) ||
								!testFaces.ContainsKey(Facing.Right) ||
								testFaces[Facing.Right] != face.Value)
								break;
							w++;
						}

						// Расширение по Y (высота) - правильно для правой грани
						bool canGrowHeight;
						do
						{
							canGrowHeight = true;
							for (int i = 0; i < w; i++)
							{
								Vector3i testPos = facePosition + new Vector3i(0, h, i);
								if (!facesList.TryGetValue(testPos, out var testFaces) ||
									!testFaces.ContainsKey(Facing.Right) ||
									testFaces[Facing.Right] != face.Value)
								{
									canGrowHeight = false;
									break;
								}
							}

							if (canGrowHeight)
								h++;
						} while (canGrowHeight && h < Chunk.SizeY);

						// Удаляем обработанные грани
						for (int i = 0; i < h; i++)
						{
							for (int j = 0; j < w; j++)
							{
								Vector3i pos = facePosition + new Vector3i(0, i, j);
								if (facesList.TryGetValue(pos, out var posFaces))
								{
									posFaces.Remove(Facing.Right);
									if (posFaces.Count == 0)
										facesList.Remove(pos);
								}
							}
						}

						AddQuad(ref vertices, facePosition, new Vector2i(w, h), face.Key, face.Value);
						break;
					}

					case Facing.Back:
					{
						int w = 1;
						int h = 1;

						// Расширение по X (ширина) - правильно для задней грани
						while (w < Chunk.SizeX)
						{
							Vector3i testPos = facePosition + new Vector3i(w, 0, 0);
							if (!facesList.TryGetValue(testPos, out var testFaces) ||
								!testFaces.ContainsKey(Facing.Back) ||
								testFaces[Facing.Back] != face.Value)
								break;
							w++;
						}

						// Расширение по Y (высота) - правильно для задней грани
						bool canGrowHeight;
						do
						{
							canGrowHeight = true;
							for (int i = 0; i < w; i++)
							{
								Vector3i testPos = facePosition + new Vector3i(i, h, 0);
								if (!facesList.TryGetValue(testPos, out var testFaces) ||
									!testFaces.ContainsKey(Facing.Back) ||
									testFaces[Facing.Back] != face.Value)
								{
									canGrowHeight = false;
									break;
								}
							}

							if (canGrowHeight)
								h++;
						} while (canGrowHeight && h < Chunk.SizeY);

						// Удаляем обработанные грани
						for (int i = 0; i < h; i++)
						{
							for (int j = 0; j < w; j++)
							{
								Vector3i pos = facePosition + new Vector3i(j, i, 0);
								if (facesList.TryGetValue(pos, out var posFaces))
								{
									posFaces.Remove(Facing.Back);
									if (posFaces.Count == 0)
										facesList.Remove(pos);
								}
							}
						}

						AddQuad(ref vertices, facePosition, new Vector2i(w, h), face.Key, face.Value);
						break;
					}

					case Facing.Front:
					{
						int w = 1;
						int h = 1;

						// Расширение по X (ширина) - правильно для передней грани
						while (w < Chunk.SizeX)
						{
							Vector3i testPos = facePosition + new Vector3i(w, 0, 0);
							if (!facesList.TryGetValue(testPos, out var testFaces) ||
								!testFaces.ContainsKey(Facing.Front) ||
								testFaces[Facing.Front] != face.Value)
								break;
							w++;
						}

						// Расширение по Y (высота) - правильно для передней грани
						bool canGrowHeight;
						do
						{
							canGrowHeight = true;
							for (int i = 0; i < w; i++)
							{
								Vector3i testPos = facePosition + new Vector3i(i, h, 0);
								if (!facesList.TryGetValue(testPos, out var testFaces) ||
									!testFaces.ContainsKey(Facing.Front) ||
									testFaces[Facing.Front] != face.Value)
								{
									canGrowHeight = false;
									break;
								}
							}

							if (canGrowHeight)
								h++;
						} while (canGrowHeight && h < Chunk.SizeY);

						// Удаляем обработанные грани
						for (int i = 0; i < h; i++)
						{
							for (int j = 0; j < w; j++)
							{
								Vector3i pos = facePosition + new Vector3i(j, i, 0);
								if (facesList.TryGetValue(pos, out var posFaces))
								{
									posFaces.Remove(Facing.Front);
									if (posFaces.Count == 0)
										facesList.Remove(pos);
								}
							}
						}

						AddQuad(ref vertices, facePosition, new Vector2i(w, h), face.Key, face.Value);
						break;
					}
				}
			}
		}

		if (vertices.Count > 0)
		{
			Mesh mesh = new Mesh();
			mesh.Create(vertices.ToArray());
			_meshes[chunkPosition] = mesh;
			VerticesCount += mesh.VerticesCount;
		}
	}

	private void AddQuad(ref List<float> vertices, Vector3 position, Vector2i size, Facing facing, BlockType blockType)
	{
		float faceTexture = ModelsContainer.Blocks[blockType][facing];
		float faceId = (int)facing;

		switch (facing)
		{
			// -X
			case Facing.Left:
			{
				vertices.AddRange([
					position.X, position.Y,          position.Z,          0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X, position.Y,          position.Z + size.X, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X, position.Y + size.Y, position.Z + size.X, 1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X, position.Y,          position.Z,          0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X, position.Y + size.Y, position.Z + size.X, 1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X, position.Y + size.Y, position.Z,          0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
				]);
				break;
			}

			// +X
			case Facing.Right:
			{
				vertices.AddRange([
					position.X + 1.0f, position.Y + size.Y, position.Z + size.X, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + 1.0f, position.Y,          position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + 1.0f, position.Y + size.Y, position.Z,          1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + 1.0f, position.Y,          position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + 1.0f, position.Y + size.Y, position.Z + size.X, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + 1.0f, position.Y,          position.Z + size.X, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
				]);
				break;
			}

			// -Z
			case Facing.Back:
			{
				vertices.AddRange([
					position.X + size.X, position.Y + size.Y, position.Z, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X,          position.Y,          position.Z, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X,          position.Y + size.Y, position.Z, 1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + size.X, position.Y + size.Y, position.Z, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + size.X, position.Y,          position.Z, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X,          position.Y,          position.Z, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
				]);
				break;
			}

			// +Z
			case Facing.Front:
			{
				vertices.AddRange([
					position.X,          position.Y + size.Y, position.Z + 1.0f, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X,          position.Y,          position.Z + 1.0f, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + size.X, position.Y,          position.Z + 1.0f, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + size.X, position.Y + size.Y, position.Z + 1.0f, 1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X,          position.Y + size.Y, position.Z + 1.0f, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
					position.X + size.X, position.Y,          position.Z + 1.0f, 1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.X, size.Y, faceId,
				]);
				break;
			}

			// -Y
			case Facing.Down:
			{
				vertices.AddRange([
					position.X + size.X, position.Y, position.Z + size.Y, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X,          position.Y, position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X + size.X, position.Y, position.Z,          1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X + size.X, position.Y, position.Z + size.Y, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X,          position.Y, position.Z + size.Y, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X,          position.Y, position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
				]);
				break;
			}

			// +Y
			case Facing.Up:
			{
				vertices.AddRange([
					position.X + size.X, position.Y + 1.0f, position.Z + size.Y, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X + size.X, position.Y + 1.0f, position.Z,          1.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X,          position.Y + 1.0f, position.Z,          1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X + size.X, position.Y + 1.0f, position.Z + size.Y, 0.0f, 1.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X,          position.Y + 1.0f, position.Z,          1.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
					position.X,          position.Y + 1.0f, position.Z + size.Y, 0.0f, 0.0f, faceTexture, 1.0f, 1.0f, 1.0f, size.Y, size.X, faceId,
				]);
				break;
			}
		}
	}

	public void RemoveMesh(Vector3i position)
	{
		VerticesCount -= _meshes[position].VerticesCount;
		_meshes[position].Delete();
		_meshes.Remove(position);
	}
	
	public void Render()
	{
		foreach (Mesh mesh in _meshes.Values)
		{
			mesh.Render();
		}
	}
}