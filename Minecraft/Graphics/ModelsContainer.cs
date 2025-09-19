using Minecraft.World;
using Newtonsoft.Json.Linq;

namespace Minecraft.Graphics;

public static class ModelsContainer
{
	public static Dictionary<BlockType, Dictionary<Facing, int>> Blocks = new();
	
	public static void LoadModels(string path)
	{
		foreach (string fileName in Directory.GetFiles(path))
		{
			Console.WriteLine($"Loading {fileName}");
			JObject obj = JObject.Parse(File.ReadAllText(fileName));
			Dictionary<Facing, int> blockTextures = new();

			JArray facings = (JArray)obj["facings"];
			for (int i = 0; i < facings.Count; i++)
				blockTextures[(Facing)i] = (int)facings[i];
			
			Blocks[(BlockType)(int)obj["block_type"]] = blockTextures;
		}
	}
}
