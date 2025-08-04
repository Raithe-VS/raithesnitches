using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace raithesnitches.src.HarmonyPatching
{
    [HarmonyPatch(typeof(InventoryBase), nameof(InventoryBase.LateInitialize))]
    public class PatchInventoryPosFromInventoryID
    {
        public static void Postfix(InventoryBase __instance)
        {
            if (__instance.Pos != null) return; // Already set

            BlockPos parsedPos = ParseBlockPosFromInventoryID(__instance.InventoryID);
            if (parsedPos != null)
            {
                __instance.Pos = parsedPos;
            }
        }

        private static BlockPos ParseBlockPosFromInventoryID(string inventoryID)
        {
            if (string.IsNullOrEmpty(inventoryID)) return null;

            // Example: "blockentity-crate-123, 45, 678"
            var parts = inventoryID.Split('-');
            if (parts.Length < 2) return null;

            var coords = parts[^1].Split(',');
            if (coords.Length != 3) return null;

            return (int.TryParse(coords[0], out int x) &&
                    int.TryParse(coords[1], out int y) &&
                    int.TryParse(coords[2], out int z))
                ? new BlockPos(x, y, z)
                : null;
        }
    }
}


