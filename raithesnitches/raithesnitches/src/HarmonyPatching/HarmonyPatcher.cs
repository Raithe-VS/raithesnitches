using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Vintagestory.API.Common;

namespace raithesnitches.src.harmonypatching
{
	public class HarmonyPatcher : ModSystem
	{
		private const string patchCode = "com.raithesnitches.patches";

		public string sidedPatchCode;

		public Harmony harmonyInstance;

		private static bool harmonyPatched;

		public override void Start(ICoreAPI api)
		{
			if (harmonyPatched)
			{
				return;
			}
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string arg = Mod?.Info.Name ?? executingAssembly.GetCustomAttribute<ModInfoAttribute>()?.Name ?? "Null";
			sidedPatchCode = string.Format("{0}.{1}.{2}", arg, "com.raithesnitches.patches", api.Side);
			harmonyInstance = new Harmony(sidedPatchCode);
			harmonyInstance.PatchAll();
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (MethodBase patchedMethod in harmonyInstance.GetPatchedMethods())
			{
				if (dictionary.ContainsKey(patchedMethod.FullDescription()))
				{
					dictionary[patchedMethod.FullDescription()]++;
				}
				else
				{
					dictionary[patchedMethod.FullDescription()] = 1;
				}
			}
			StringBuilder stringBuilder = new StringBuilder($"{arg}: Harmony Patched Methods: ").AppendLine();
			stringBuilder.AppendLine("[");
			foreach (KeyValuePair<string, int> item in dictionary)
			{
				stringBuilder.AppendLine($"  {item.Value}: {item.Key}");
			}
			stringBuilder.Append("]");
			api.Logger.Notification(stringBuilder.ToString());
			harmonyPatched = true;
		}

		public override void Dispose()
		{
			Harmony obj = harmonyInstance;
			if (obj != null)
			{
				obj.UnpatchAll(sidedPatchCode);
			}
			harmonyPatched = false;
		}
	}
}
