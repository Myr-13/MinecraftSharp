using Minecraft.World;
using Minecraft.World.Blocks;
using OpenTK.Mathematics;

namespace Minecraft.Graphics;

public class WorldRenderer : IDisposable
{
    private Dictionary<Vector3i, Mesh> _meshes = new();
    private HashSet<Vector3i> _pendingMeshes = new();
    private MeshWorker _meshWorker = new();
    public long VerticesCount { get; private set; }
    public World.World World { get; set; }

    public void EnqueueChunkMesh(Vector3i chunkPosition)
    {
        if (_pendingMeshes.Contains(chunkPosition) || _meshes.ContainsKey(chunkPosition))
            return;

        if (!World.Chunks.TryGetValue(chunkPosition, out Chunk chunk))
            return;

        _pendingMeshes.Add(chunkPosition);
        _meshWorker.EnqueueTask(new MeshTask
        {
            ChunkPosition = chunkPosition,
            Blocks = chunk.CopyBlocks()
        });
    }

    public void ProcessResults(Vector3 cameraPosition)
    {
        while (_meshWorker.TryDequeueResult(out MeshResult result))
        {
            _pendingMeshes.Remove(result.ChunkPosition);

            if (!result.Success || result.Vertices.Length == 0)
                continue;

            if (_meshes.ContainsKey(result.ChunkPosition))
                RemoveMesh(result.ChunkPosition);

            Mesh mesh = new Mesh();
            mesh.Create(result.Vertices);
            _meshes[result.ChunkPosition] = mesh;
            VerticesCount += mesh.VerticesCount;
        }

        List<Vector3i> toRemove = new();
        foreach (Vector3i pos in _meshes.Keys)
            if (World.IsOutsideRenderDistance(pos, cameraPosition))
                toRemove.Add(pos);

        foreach (Vector3i pos in toRemove)
            RemoveMesh(pos);

        foreach (Vector3i pos in World.Chunks.Keys)
        {
            bool hasMesh = _meshes.ContainsKey(pos);
            bool isPending = _pendingMeshes.Contains(pos);
            bool isOutside = World.IsOutsideRenderDistance(pos, cameraPosition);

            if (isOutside || isPending)
                continue;

            if (!hasMesh)
            {
                EnqueueChunkMesh(pos);
            }
            else if (World.Chunks[pos].Dirty)
            {
                World.Chunks[pos].Dirty = false;
                _pendingMeshes.Add(pos);
                RemoveMesh(pos);
                _meshWorker.EnqueueTask(new MeshTask
                {
                    ChunkPosition = pos,
                    Blocks = World.Chunks[pos].CopyBlocks()
                });
            }
        }
    }

    private void RemoveMesh(Vector3i position)
    {
        if (!_meshes.ContainsKey(position))
            return;
        VerticesCount -= _meshes[position].VerticesCount;
        _meshes[position].Delete();
        _meshes.Remove(position);
    }

    public void Render()
    {
        foreach (Mesh mesh in _meshes.Values)
            mesh.Render();
    }

    public void Dispose()
    {
        _meshWorker.Dispose();
        foreach (Mesh mesh in _meshes.Values)
            mesh.Delete();
        _meshes.Clear();
    }
}
