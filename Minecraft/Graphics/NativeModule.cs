using System.Diagnostics;
using System.Runtime.InteropServices;
using Minecraft.World;
using OpenTK.Mathematics;

namespace Minecraft.Graphics;

public class NativeModule
{
	[DllImport("MinecraftSharpModule.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "greedy_meshing")]
	private static extern unsafe void GreedyMeshing(int x, int y, int z, float **vertices, int *arraySize);
	
	[DllImport("MinecraftSharpModule.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "set_chunk")]
	private static extern void SetChunk(int x, int y, int z, IntPtr ids);
	
	[DllImport("MinecraftSharpModule.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "set_constants")]
	public static extern void SetConstants(int chunkX, int chunkY, int chunkZ);
	
	[DllImport("MinecraftSharpModule.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "free_memory")]
	private static extern void FreeMemory(IntPtr vertices);
	
	[DllImport("MinecraftSharpModule.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "set_model")]
	public static extern void SetModel(int blockId, int[] facings);

	public static unsafe List<float> Meshing(Vector3i chunkPosition, Chunk chunk)
	{
		float* ptrVertices = null;
		float** ptrPtrVertices = &ptrVertices;
		int verticesSize = 0;
		
		GreedyMeshing(chunkPosition.X, chunkPosition.Y, chunkPosition.Z, ptrPtrVertices, &verticesSize);

		float[] tmpVertices = new float[verticesSize];
		Marshal.Copy((IntPtr)ptrVertices, tmpVertices, 0, verticesSize);
		List<float> vertices = new(tmpVertices);
		
		FreeMemory((IntPtr)ptrVertices);

		return vertices;
	}

	public static unsafe void SetChunk(Vector3i chunkPosition, Chunk chunk)
	{
		fixed (BlockType* blockType = chunk._blocks)
			SetChunk(chunkPosition.X, chunkPosition.Y, chunkPosition.Z, (IntPtr)blockType);
	}

	public static unsafe float[] MeshChunk(Vector3i chunkPosition, BlockType[] blocks)
	{
		fixed (BlockType* ptr = blocks)
			SetChunk(chunkPosition.X, chunkPosition.Y, chunkPosition.Z, (IntPtr)ptr);

		float* ptrVertices = null;
		float** ptrPtrVertices = &ptrVertices;
		int verticesSize = 0;

		GreedyMeshing(chunkPosition.X, chunkPosition.Y, chunkPosition.Z, ptrPtrVertices, &verticesSize);

		float[] tmpVertices = new float[verticesSize];
		Marshal.Copy((IntPtr)ptrVertices, tmpVertices, 0, verticesSize);
		FreeMemory((IntPtr)ptrVertices);

		return tmpVertices;
	}
}
