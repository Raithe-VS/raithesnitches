using HarmonyLib;
using raithesnitches.src.Violations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace raithesnitches.src.HarmonyPatching
{    

    [HarmonyPatch(typeof(ItemSlot), "TakeOut")]
    public static class ItemSlotPatchTakeOut
    {
        public static void Postfix(ItemSlot __instance, ref ItemStack __result)
        {

            if (__result == null || __result.StackSize == 0 || __instance.Inventory?.Api == null)
                return;

            var api = __instance.Inventory.Api;
            if (api.Side == EnumAppSide.Client) return;
            var pos = __instance.Inventory.Pos;

            if (pos == null) return;

            var snitchMod = api.ModLoader.GetModSystem<SnitchesModSystem>();
            if (snitchMod == null) return;

            var players = __instance.Inventory.openedByPlayerGUIds;
            foreach (var playerUID in players)
            {
                var player = api.World.PlayerByUid(playerUID);
                if (player == null) continue;

                if (!snitchMod.trackedPlayers.TryGetValue(playerUID, out var snitches)) continue;

                foreach (var snitch in snitches)
                {
                    if (snitch == null || pos.HorizontalManhattenDistance(snitch.Pos) > snitch.Radius)
                        continue;

                    var block = api.World.BlockAccessor.GetBlock(pos);
                    var violation = new SnitchViolation(
                        EnumViolationType.CollectibleTaken,
                        player.Entity,
                        pos,
                        api.World.Calendar.PrettyDate(),
                        api.World.Calendar.ElapsedDays,
                        api.World.Calendar.ElapsedSeconds,
                        api.World.Calendar.Year,
                        block,
                        null,
                        __result.Collectible,
                        __result.StackSize
                    );

                    snitch.AddViolation(violation);
                }
            }
        }
    }    
}
