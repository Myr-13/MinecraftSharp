using Minecraft.World;
using OpenTK.Mathematics;

namespace Minecraft.Graphics;

public class MeshTask
{
    public Vector3i ChunkPosition { get; init; }
    public BlockType[] Blocks { get; init; }
}
