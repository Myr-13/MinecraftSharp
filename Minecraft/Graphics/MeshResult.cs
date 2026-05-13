using OpenTK.Mathematics;

namespace Minecraft.Graphics;

public class MeshResult
{
    public Vector3i ChunkPosition { get; init; }
    public float[] Vertices { get; init; } = [];
    public bool Success { get; init; }
}
