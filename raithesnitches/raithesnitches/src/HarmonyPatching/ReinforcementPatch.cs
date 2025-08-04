using HarmonyLib;
using raithesnitches.src.BlockEntities;
using raithesnitches.src.Violations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace raithesnitches.src.HarmonyPatching
{
    [HarmonyPatch(typeof(ModSystemBlockReinforcement), "StrengthenBlock")]
    public class ReinforcementPatch
    {
        public static void Postfix(bool __result, BlockPos pos, IPlayer byPlayer)
        {
            if (__result) {
                SnitchesModSystem SnitchMod = byPlayer.Entity.Api.ModLoader.GetModSystem<SnitchesModSystem>();
                ICoreServerAPI api = byPlayer.Entity.Api as ICoreServerAPI;
                if (SnitchMod != null && SnitchMod.trackedPlayers.TryGetValue(byPlayer.PlayerUID, out List<BlockEntitySnitch> snitches))
                {
                    if (snitches != null)
                    {
                        foreach (BlockEntitySnitch s in snitches)
                        {
                            string prettyDate = api.World.Calendar.PrettyDate();
                            double day = api.World.Calendar.ElapsedDays;
                            long time = api.World.Calendar.ElapsedSeconds;
                            int year = api.World.Calendar.Year;

                            SnitchViolation violation = new SnitchViolation(EnumViolationType.ReinforcementPlaced, byPlayer.PlayerUID, pos, prettyDate, day, time, year, byPlayer.CurrentBlockSelection.Block.GetPlacedBlockName(api.World, pos));

                            s.AddViolation(violation);
                        }
                    }
                }


            }

        }
    }
}
