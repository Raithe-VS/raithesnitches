using raithesnitches.src.Config;
using raithesnitches.src.Players;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using raithesnitches.src.Violations;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using raithesnitches.src.Constants;


namespace raithesnitches.src.BlockEntities
{
    public class BlockEntitySnitch : BlockEntity
    {
        SnitchPlayer snitchPlayer;

        public int Radius { get; private set; }
        public int VertRange { get; private set; }
        public int TrueSightRange { get; private set; }
        public int MaxPaperLog { get; private set; }
        public int MaxBookLog { get; private set; }
		public int MaxSnitchLog { get; private set; }
		public float SnitchDownloadTime { get; private set; }
        public bool Sneakable { get; private set; }
        public string CurrentOwnerUID { get; private set; }
		       
		public int violationCount { get; set; }

        private List<string> playersPinged;
        private List<string> playersTracked;

        private SnitchesModSystem snitchMod;
        private ModSystemEditableBook bookMod;
        private ModSystemBlockReinforcement reinforceMod;

        long? OnPlayerEnterListenerID;

		public bool Activated { get; private set; } = false;

        private SnitchesServerConfig config => SnitchesModSystem.config;

		ViolationLogger violationLogger;

		public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
		{
			base.FromTreeAttributes(tree, worldAccessForResolve);
		    Activated = tree.GetBool("activated");
			
			CurrentOwnerUID = tree.GetString("currentOwnerUID");				

			int playersTrackedCount = tree.GetInt("playersTrackedCount");
						
			var playersTrackedTree = tree.GetOrAddTreeAttribute("playersTracked");

			violationCount = tree.GetInt("violationCount");
					
			if (Activated)
			{
				playersTracked = new List<string>();

				for (int counter = 0; counter < playersTrackedCount; counter++)
				{
					playersTracked.Add(playersTrackedTree.GetString("player" + counter));
				}

			}
		}

		public override void ToTreeAttributes(ITreeAttribute tree)
		{
			base.ToTreeAttributes(tree);
			tree.SetBool("activated", Activated);
			tree.SetString("currentOwnerUID", CurrentOwnerUID);				
			tree.SetInt("playersTrackedCount", playersTracked.Count);
			tree.SetInt("violationCount", violationCount);
			
			var playersTrackedTree = tree.GetOrAddTreeAttribute("playersTracked");	
			
			if (Activated && playersTracked.Count > 0)
			{
				int counter = 0;
				foreach (var p in playersTracked)
				{
					//ITreeAttribute player = playersTrackedTree.GetOrAddTreeAttribute("player" + counter.ToString());
					playersTrackedTree.SetString("player" + counter, p);
				}

			}

		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			snitchMod = api.ModLoader.GetModSystem<SnitchesModSystem>();
			bookMod = api.ModLoader.GetModSystem<ModSystemEditableBook>();
			reinforceMod = Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();

			Radius = config.snitchRadius;
			VertRange = config.snitchVerticalRange;

			Sneakable = config.snitchSneakable;
			TrueSightRange = Sneakable == true ? (int)(Radius * config.snitchTruesightRange) : Radius;

			MaxBookLog = config.maxBookLog;
			MaxPaperLog = config.maxPaperLog;
			MaxSnitchLog = config.snitchMaxLog;
			SnitchDownloadTime = config.snitchDownloadTime;

			if (api.Side == EnumAppSide.Server && Activated)
			{
				TryActivate();
			}

			playersTracked = new List<string>();

			if (api.Side == EnumAppSide.Server)
			{
				violationLogger = snitchMod.violationLogger;
			}

		}
		public override void OnBlockUnloaded()
		{
			base.OnBlockUnloaded();
			RemoveSnitchesFromTracker();
		}

		public override void OnBlockRemoved()
		{
			if (Api.Side == EnumAppSide.Server)
			{
				violationLogger.ClearViolationChunkData(this);
			}
			RemoveSnitchesFromTracker();
			base.OnBlockRemoved();						
		}

		//public override void OnBlockBroken(IPlayer byPlayer = null)
		//{
		//	base.OnBlockBroken(byPlayer);
		//	RemoveSnitches();			
		//}

		public override void OnBlockPlaced(ItemStack byItemStack = null)
		{
			base.OnBlockPlaced(byItemStack);
			//if (Api.Side == EnumAppSide.Server)
			//{
			//	violationLogger.ClearViolationChunkData(this);
			//}
		}

		public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
		{
			base.GetBlockInfo(forPlayer, dsc);

			if (Activated)
			{
				dsc.AppendLine("This Snitch is Activated! The watchful eye of Big Brother!");
			}

			if (IsOwner(forPlayer.PlayerUID))
			{

				dsc.AppendLine("Snitch_" + Pos.ToLocalPosition(Api).ToString());

				foreach (string player in playersTracked)
				{
					if (player != null)
					{
						dsc.AppendLine(Api.World.PlayerByUid(player).PlayerName + " currently being tracked!");
					}
				}

				dsc.AppendLine($"Current Violations: {violationCount} / {MaxSnitchLog} ");
			}

		}

		public bool OnInteract(IPlayer byPlayer, float secondsUsed)
		{
			if (!Activated && Api.Side == EnumAppSide.Server && byPlayer.Entity.Controls.ShiftKey && TryActivate())
			{
				CurrentOwnerUID = byPlayer.PlayerUID;				
				MarkDirty();
				return false;
			};			
			

			if (Activated && byPlayer.Entity.Controls.CtrlKey)
			{
				string error = "";		

				if (CanWriteViolations(byPlayer, ref error))
				{
					if(secondsUsed < SnitchDownloadTime)
					{
						if (Api.Side == EnumAppSide.Client && !byPlayer.Entity.AnimManager.IsAnimationActive(SnitchesConstants.SNITCH_DOWNLOAD_ANIMATION))
						{
							byPlayer.Entity.StartAnimation(SnitchesConstants.SNITCH_DOWNLOAD_ANIMATION);
						}
						return true;
					} else
					{
                        if (Api.Side == EnumAppSide.Server)
                        {
                            WriteViolations(byPlayer);
                            MarkDirty();
							return false;
                        } else
						{
							
							return true;
						}							
                    }					
				} else
				{
					(Api as ICoreServerAPI)?.SendIngameError(byPlayer as IServerPlayer, error);
					return false;
				}
			}

			return true;

		}

		private bool CanWriteViolations(IPlayer byPlayer, ref string errorcode)
		{
            if (!HasPermission(byPlayer))
            {
                errorcode = "You do not have permission to use this snitch, activity logged!";
                return false;
            }

            ItemSlot bookSlot = byPlayer.Entity.ActiveHandItemSlot;
            ItemSlot penSlot = byPlayer.Entity.LeftHandItemSlot;
            if (!(bookSlot.Itemstack?.Item is ItemBook))
            {
                errorcode = "You need something to write in! Try a book or a piece of parchment!";
                return false;
            }

            if (penSlot.Empty || !(penSlot.Itemstack.Class == EnumItemClass.Item))
            {
                errorcode = "You need something to write with in your offhand! Try an inkquill!";
                return false;
            }
            if (!(penSlot.Itemstack.Item.Attributes["writingTool"].Exists) || penSlot.Itemstack.Item.Attributes["writingTool"].AsBool() == false)
            {
                errorcode = "You need something to write with in your offhand! Try an inkquill!";
                return false;
            }

            //if(bookSlot.Itemstack.Attributes.GetString("signedbyUID") != snitchPlayer.PlayerUID)
            //{
            //	errorcode = "This writing media has been bound to another Snitch";
            //	return false;
            //}

            return true;
		}


		// Maybe allow callback to allow the interact to happen after Book log is pulled
		private void WriteViolations(IPlayer byPlayer)
		{	
			snitchPlayer = new SnitchPlayer()
			{
				playerName = "Snitch_" + Pos.ToLocalPosition(Api).ToString(),
				playerUID = "Snitch_" + Pos.ToLocalPosition(Api).ToString(),
				entityPlayer = byPlayer.Entity
			};

			ItemSlot bookSlot = byPlayer.Entity.ActiveHandItemSlot;
			ItemSlot penSlot = byPlayer.Entity.LeftHandItemSlot;			

			

			bookMod.BeginEdit(snitchPlayer, bookSlot);

			string text = "";
			string title = "Violations pulled on " + Api.World.Calendar.PrettyDate();
			int maxLogSize = 0;

			if (bookSlot.Itemstack.Collectible.Code.ToString().Contains("parchment")) maxLogSize = MaxPaperLog;
			if (bookSlot.Itemstack.Collectible.Code.ToString().Contains("book")) maxLogSize = MaxBookLog;
						
			var log = violationLogger.GetViolations(maxLogSize, this);

			int tempCount = log.Count - 1;			

			for (int i = 0; (i <= maxLogSize && i <= tempCount); i++)
			{
				SnitchViolation violation = log.Dequeue();				

				text += (violation.LogbookFormat(Api) + "\n");
			}			

			bookMod.EndEdit(snitchPlayer, text, title, true);		
		}		

		public void AddViolation(SnitchViolation violation)
		{
			violationLogger.AddViolation(violation, this);
		}

		private bool TryActivate()
		{
			if (OnPlayerEnterListenerID == null)
			{
				OnPlayerEnterListenerID = RegisterGameTickListener(OnPingPlayers, 500);
			}
			if (Activated)
			{
				MarkDirty();
				return false;
			}

			Activated = true;
			MarkDirty();

			return true;
		}

		private void OnPingPlayers(float obj)
		{
			// Gets all potential players to ping
			List<IPlayer> players = Api.World.GetPlayersAround(Pos.ToVec3d(), Radius, VertRange, (IPlayer player) =>
			{
				return ShouldPingPlayer(player);

			}).ToList<IPlayer>();
			playersPinged = new List<string>();

			// For each player that should be pinged, we add that player to the tracked players list along with its Block Entity
			// If that player was not already being tracked by a snitch, a Tresspass Violation is added
			foreach (IPlayer player in players)
			{

				if (snitchMod.trackedPlayers.TryGetValue(player.PlayerUID, out List<BlockEntitySnitch> snitches))
				{
					if (!snitches.Contains(this))
					{
						snitches.Add(this);
					}
				}
				else
				{
					var sn = new List<BlockEntitySnitch>();
					sn.Add(this);
					snitchMod.trackedPlayers.Add(player.PlayerUID, sn);
				}
				playersPinged.Add(player.PlayerUID);

				if (!playersTracked.Contains(player.PlayerUID))
				{
					playersTracked.Add(player.PlayerUID);
					
					if (Api.Side == EnumAppSide.Server)
					{
						string prettyDate = Api.World.Calendar.PrettyDate();
						double day = Api.World.Calendar.ElapsedDays;
						long time = Api.World.Calendar.ElapsedSeconds;
						int year = Api.World.Calendar.Year;
						AddViolation(new SnitchViolation(EnumViolationType.Trespassed, player as IServerPlayer, player.Entity.Pos.AsBlockPos, prettyDate, day, time, year));
					}
				}
			}

			// Now we compare the players tracked list with players that should be pinged list
			// For each player that is in the players tracked list and not in the pinged list, we add an escape violation
			List<string> ps = new List<string>();

			foreach (string playerUID in playersTracked)
			{
				if (playersPinged.Contains(playerUID)) { continue; }

				if (snitchMod.trackedPlayers.TryGetValue(playerUID, out List<BlockEntitySnitch> snitches))
				{
					var player = Api.World.PlayerByUid(playerUID);

					string prettyDate = Api.World.Calendar.PrettyDate();
					double day = Api.World.Calendar.ElapsedDays;
					long time = Api.World.Calendar.ElapsedSeconds;
					int year = Api.World.Calendar.Year;
					AddViolation(new SnitchViolation(EnumViolationType.Escaped, player as IServerPlayer, player.Entity.Pos.AsBlockPos, prettyDate, day, time, year));

					snitches.Remove(this);
				}
				ps.Add(playerUID);
			}

			foreach (string playerUID in ps)
			{
				playersTracked.Remove(playerUID);
			}

			if (Api.Side == EnumAppSide.Server)
			{
				MarkDirty();
			}

		}

		private bool ShouldPingPlayer(IPlayer player)
		{
			if (!IsOwner(player)) return false;

			//if (reinforceMod.IsReinforced(Pos))
			//{
			//	if (player.GetGroup(reinforceMod.GetReinforcment(Pos).GroupUid) != null)
			//	{
			//		return false;
			//	}
			//}

			if (Sneakable && player.Entity.Controls.Sneak && Pos.DistanceTo(player.Entity.Pos.AsBlockPos) > TrueSightRange) return false;

			return true;
		}

		private void RemoveSnitchesFromTracker()
		{
			foreach (List<BlockEntitySnitch> snitches in snitchMod.trackedPlayers.Values)
			{
				if (snitches.Contains(this))
				{
					snitches.Remove(this);
				}
			}
		}

		private bool IsOwner(string playerUID)
		{
			if (playerUID == CurrentOwnerUID)
			{
				return true;
			}
			else return false;
		}

		private bool IsOwner(IPlayer player)
		{
			if(player.PlayerUID == CurrentOwnerUID)
			{
				return true;
			}
			else return false;

		}

		private bool HasPermission(IPlayer player)
		{
			if(IsOwner(player)) { return true; }
			
			//PlayerGroupMembership group;

			if(reinforceMod.IsReinforced(Pos) && player.GetGroup(reinforceMod.GetReinforcment(Pos).GroupUid) != null)
			{
				return true;
			}


			return false;
		}

		
	}
}
