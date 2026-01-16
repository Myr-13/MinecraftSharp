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
			RemoveMesh(chunkPosition);
		
		List<float> vertices = NativeModule.Meshing(chunkPosition, world.Chunks[chunkPosition]);
		
		if (vertices.Count > 0)
		{
			Mesh mesh = new Mesh();
			mesh.Create(vertices.ToArray());
			_meshes[chunkPosition] = mesh;
			VerticesCount += mesh.VerticesCount;
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