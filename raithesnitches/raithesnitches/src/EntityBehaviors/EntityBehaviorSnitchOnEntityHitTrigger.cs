using raithesnitches.src.BlockEntities;
using raithesnitches.src.Violations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using raithesnitches;

namespace raithesnitches.src.EntityBehaviors
{
	
	public class EntityBehaviorSnitchOnEntityHitTrigger : EntityBehavior
	{
		public EntityBehaviorSnitchOnEntityHitTrigger(Entity entity) : base(entity)
		{
		}

		public override string PropertyName()
		{
			return "SnitchOnEntityHitTrigger";
		}

		public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
		{
			if (entity.Api.Side == EnumAppSide.Server && damageSource.GetCauseEntity() != null && damageSource.GetCauseEntity() is EntityPlayer)
			{
				IServerPlayer byPlayer = (damageSource.GetCauseEntity() as EntityPlayer).Player as IServerPlayer;
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

							SnitchViolation violation = new SnitchViolation(EnumViolationType.EntityHit, byPlayer, entity.Pos.AsBlockPos, prettyDate, day, time, year, null, entity);

							s.AddViolation(violation);
						}
					}
				}
			}
		}
	}
}
