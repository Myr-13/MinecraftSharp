using OpenTK.Mathematics;

namespace Minecraft;

public static class MathUtils
{
	public static Vector3i FloorVector(Vector3 v)
	{
		return new Vector3i((int)Math.Floor(v.X), (int)Math.Floor(v.Y), (int)Math.Floor(v.Z));
	}
}
