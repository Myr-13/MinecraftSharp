using Newtonsoft.Json.Linq;

namespace Minecraft.Graphics;

public static class ModelsContainer
{
	public static void LoadModels(string path)
	{
		foreach (string fileName in Directory.GetFiles(path))
		{
			Console.WriteLine($"Loading {fileName}");
			JObject obj = JObject.Parse(File.ReadAllText(fileName));
			int[] facings = new int[6];

			JArray jsonFacings = (JArray)obj["facings"];
			for (int i = 0; i < (int)Facing.Count; i++)
				facings[i] = (int)jsonFacings[i];
			
			NativeModule.SetModel((int)obj["block_type"], facings);
		}
	}
}
