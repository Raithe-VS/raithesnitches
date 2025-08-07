using raithesnitches.src.BlockEntities;
using raithesnitches.src.Violations;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using raithesnitches;


namespace raithesnitches.src.Events
{
	public static class SnitchEventsServer
	{
		//Violations for Entities being hit is handled inside of the EntityBehaviorSnitchEntityHitTrigger
		//Violations for Reinforcing with a plumb and square will be handled inside of the plumb and square



		/// <summary>
		/// Logs a violation for Breaking a block and breaking a level of reinforcement
		/// </summary>
		/// <param name="byPlayer"></param>
		/// <param name="oldblockId"></param>
		/// <param name="blockSel"></param>
		public static void DidBreakBlock(IServerPlayer byPlayer, int oldblockId, BlockSelection blockSel)
		{
			ICoreAPI sapi = byPlayer.Entity.Api;
			SnitchesModSystem SnitchMod = sapi.ModLoader.GetModSystem<SnitchesModSystem>();

			if (SnitchMod.trackedPlayers.TryGetValue(byPlayer.PlayerUID, out List<BlockEntitySnitch> snitches))
			{
				if (snitches != null)
				{
					foreach (BlockEntitySnitch s in snitches)
					{
						string prettyDate = sapi.World.Calendar.PrettyDate();
						double day = sapi.World.Calendar.ElapsedDays;
						long time = sapi.World.Calendar.ElapsedSeconds;
						int year = sapi.World.Calendar.Year;

						// Check if reinforced and add Reinforcement Breaking Violation
						if (sapi.World.BlockAccessor.GetBlock(blockSel.Position).Id == oldblockId)
						{
							SnitchViolation violation = new SnitchViolation(EnumViolationType.ReinforcementBroke, byPlayer, blockSel.Position, prettyDate, day, time, year, sapi.World.GetBlock(oldblockId));

							s.AddViolation(violation);

							

						}
						else
						{
							SnitchViolation violation = new SnitchViolation(EnumViolationType.BlockBroke, byPlayer, blockSel.Position, prettyDate, day, time, year, sapi.World.GetBlock(oldblockId));

							s.AddViolation(violation);
							
						}
					}
				}
			}
		}

		/// <summary>
		/// Logs a violation for placing a block
		/// </summary>
		/// <param name="byPlayer"></param>
		/// <param name="oldblockId"></param>
		/// <param name="blockSel"></param>
		/// <param name="withItemStack"></param>
		public static void DidPlaceBlock(IServerPlayer byPlayer, int oldblockId, BlockSelection blockSel, ItemStack withItemStack)
		{
			ICoreAPI api = byPlayer.Entity.Api;
			SnitchesModSystem SnitchMod = api.ModLoader.GetModSystem<SnitchesModSystem>();

			ItemSlot itemSlot = byPlayer?.InventoryManager?.GetHotbarInventory()?[10];
			EnumHandHandling handling = EnumHandHandling.Handled;
			BlockPos position = blockSel.Position;
			(itemSlot?.Itemstack?.Item as ItemPlumbAndSquare)?.OnHeldInteractStart(itemSlot, byPlayer.Entity, blockSel, null, firstEvent: true, ref handling);

			if (SnitchMod.trackedPlayers.TryGetValue(byPlayer.PlayerUID, out List<BlockEntitySnitch> snitches))
			{
				if (snitches != null)
				{
					foreach (BlockEntitySnitch s in snitches)
					{
						string prettyDate = api.World.Calendar.PrettyDate();
						double day = api.World.Calendar.ElapsedDays;
						long time = api.World.Calendar.ElapsedSeconds;
						int year = api.World.Calendar.Year;

						SnitchViolation violation = new SnitchViolation(EnumViolationType.BlockPlaced, byPlayer, blockSel.Position, prettyDate, day, time, year, withItemStack.Block);

						s.AddViolation(violation);
						
					}
				}
			}
		}


		/// <summary>
		/// Logs a violation for using a block
		/// </summary>
		/// <param name="byPlayer"></param>
		/// <param name="blockSel"></param>
		public static void DidUseBlock(IServerPlayer byPlayer, BlockSelection blockSel)
		{			
			ICoreAPI sapi = byPlayer.Entity.Api;					

			SnitchesModSystem SnitchMod = sapi.ModLoader.GetModSystem<SnitchesModSystem>();

			if (SnitchMod.trackedPlayers.TryGetValue(byPlayer.PlayerUID, out List<BlockEntitySnitch> snitches))
			{
				if (snitches != null)
				{
					foreach (BlockEntitySnitch s in snitches)
					{
						string prettyDate = sapi.World.Calendar.PrettyDate();
						double day = sapi.World.Calendar.ElapsedDays;
						long time = sapi.World.Calendar.ElapsedSeconds;
						int year = sapi.World.Calendar.Year;

						SnitchViolation violation = new SnitchViolation(EnumViolationType.BlockUsed, byPlayer, blockSel.Position, prettyDate, day, time, year, sapi.World.GetBlockAccessor(false, false, false).GetBlock(blockSel.Position));

						s.AddViolation(violation);
						
					}
				}
			}
		}				


		/// <summary>
		/// Logs a violation for an entity dying
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="damageSource"></param>
		public static void OnEntityDeath(Entity entity, DamageSource damageSource)
		{		
			if (damageSource == null) return;
			if (damageSource.GetCauseEntity() == null || !(damageSource.GetCauseEntity() is EntityPlayer)) return;
			IServerPlayer byPlayer = (damageSource.SourceEntity as EntityPlayer).Player as IServerPlayer;
			ICoreAPI api = byPlayer.Entity.Api;
			SnitchesModSystem SnitchMod = api.ModLoader.GetModSystem<SnitchesModSystem>();

			if (SnitchMod.trackedPlayers.TryGetValue(byPlayer.PlayerUID, out List<BlockEntitySnitch> snitches))
			{
				if (snitches != null)
				{
					foreach (BlockEntitySnitch s in snitches)
					{
						string prettyDate = api.World.Calendar.PrettyDate();
						double day = api.World.Calendar.ElapsedDays;
						long time = api.World.Calendar.ElapsedSeconds;
						int year = api.World.Calendar.Year;

						SnitchViolation violation = new SnitchViolation(EnumViolationType.EntityKilled, byPlayer, entity.Pos.AsBlockPos, prettyDate, day, time, year, null, entity);

						s.AddViolation(violation);						
					}
				}
			}
		}

        internal static void OnEntitySpawn(Entity entity)
        {
			if (entity is EntityPlayer) { 
				IServerPlayer byPlayer = (entity as EntityPlayer).Player as IServerPlayer;
				ICoreAPI api = entity.Api;
                SnitchesModSystem SnitchMod = api.ModLoader.GetModSystem<SnitchesModSystem>();

				foreach (var snitch in SnitchMod.loadedSnitches)
				{
					if(entity.Pos.AsBlockPos.DistanceTo(snitch.Pos) <= snitch.Radius)
					{
                        string prettyDate = api.World.Calendar.PrettyDate();
                        double day = api.World.Calendar.ElapsedDays;
                        long time = api.World.Calendar.ElapsedSeconds;
                        int year = api.World.Calendar.Year;

                        SnitchViolation violation = new SnitchViolation(EnumViolationType.PlayerSpawned, byPlayer, entity.Pos.AsBlockPos, prettyDate, day, time, year, null, null);

                        snitch.AddViolation(violation);
                    }
				}               

            }


        }        
    }
}
