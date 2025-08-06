using raithesnitches.src.Config;
using raithesnitches.src.Constants;
using raithesnitches.src.Players;
using raithesnitches.src.Violations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;


namespace raithesnitches.src.BlockEntities
{
    using raithesnitches.src.GUI;

    /// <summary>
    /// A block entity that monitors nearby Seraph activity and logs violations such as trespassing, block interaction,
    /// or item theft. Requires a writing medium and writing tool to extract logs. Integrates with reinforcement groups
    /// and editable book systems. Supports activation, tracking, and log serialization.
    /// </summary>
    using System;
    using Vintagestory.API.Client;
    using Vintagestory.API.Util;

    public class BlockEntitySnitch : BlockEntity
    {
        // --- Configuration ---
        public int Radius { get; private set; }
        public int VertRange { get; private set; }
        public int TrueSightRange { get; private set; }
        public int MaxPaperLog { get; private set; }
        public int MaxBookLog { get; private set; }
        public int MaxSnitchLog { get; private set; }
        public float SnitchDownloadTime { get; private set; }
        public bool Sneakable { get; private set; }

        // --- Ownership ---
        public string CurrentOwnerUID { get; private set; }
        public int violationCount { get; set; }
        public bool Activated { get; private set; } = false;

        // --- Player Tracking ---
        private List<string> playersTracked = new();
        private List<string> playersPinged = new();

        // --- Mod Systems ---
        private SnitchesModSystem snitchMod;
        private ModSystemEditableBook bookMod;
        private ModSystemBlockReinforcement reinforceMod;
        private ViolationLogger violationLogger;

        private long? OnPlayerEnterListenerID;

        private SnitchPlayer snitchPlayer;

        // --- Violation Settings ---
        private EnumViolationType enabledViolationFlags = (EnumViolationType)(-1); // All flags on by default

        protected GuiSnitch clientDialog;

        private SnitchesServerConfig config => SnitchesModSystem.config;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            InitializeMods(api);
            InitializeConfig();
            if (api.Side == EnumAppSide.Server && Activated) TryActivate();
        }

        private void InitializeMods(ICoreAPI api)
        {
            snitchMod = api.ModLoader.GetModSystem<SnitchesModSystem>();
            bookMod = api.ModLoader.GetModSystem<ModSystemEditableBook>();
            reinforceMod = api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();
            if (api.Side == EnumAppSide.Server) violationLogger = snitchMod.violationLogger;
        }

        private void InitializeConfig()
        {
            Radius = config.snitchRadius;
            VertRange = config.snitchVerticalRange;
            Sneakable = config.snitchSneakable;
            TrueSightRange = Sneakable ? (int)(Radius * config.snitchTruesightRange) : Radius;
            MaxBookLog = config.maxBookLog;
            MaxPaperLog = config.maxPaperLog;
            MaxSnitchLog = config.snitchMaxLog;
            SnitchDownloadTime = config.snitchDownloadTime;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            Activated = tree.GetBool("activated");
            CurrentOwnerUID = tree.GetString("currentOwnerUID");
            violationCount = tree.GetInt("violationCount");
            enabledViolationFlags = (EnumViolationType)tree.GetInt("enabledViolationFlags");

            playersTracked.Clear();
            int count = tree.GetInt("playersTrackedCount");
            var trackedTree = tree.GetTreeAttribute("playersTracked");
            if (trackedTree != null)
            {
                for (int i = 0; i < count; i++)
                {
                    playersTracked.Add(trackedTree.GetString("player" + i));
                }
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBool("activated", Activated);
            tree.SetString("currentOwnerUID", CurrentOwnerUID);
            tree.SetInt("violationCount", violationCount);
            tree.SetInt("enabledViolationFlags", (int)enabledViolationFlags);
            tree.SetInt("playersTrackedCount", playersTracked.Count);

            if (Activated && playersTracked.Count > 0)
            {
                var trackedTree = tree.GetOrAddTreeAttribute("playersTracked");
                for (int i = 0; i < playersTracked.Count; i++)
                {
                    trackedTree.SetString("player" + i, playersTracked[i]);
                }
            }
        }

        public bool OnInteract(IPlayer byPlayer, float secondsUsed)
        {
            if (!Activated && Api.Side == EnumAppSide.Server && byPlayer.Entity.Controls.ShiftKey)
            {
                CurrentOwnerUID = byPlayer.PlayerUID;
                TryActivate();
                MarkDirty();
                return false;
            }

            if (Activated && byPlayer.Entity.Controls.CtrlKey)
            {
                return HandleLogWriting(byPlayer, secondsUsed);
            }

            if(Activated && Api.Side == EnumAppSide.Client && IsOwner(byPlayer) && byPlayer.Entity.Controls.ShiftKey) {

                if (clientDialog != null)
                {
                    //clientDialog.TryClose();
                    //clientDialog.Dispose();
                    //clientDialog = null;
                    return true;
                }

                clientDialog = new GuiSnitch(Pos, Api as ICoreClientAPI, enabledViolationFlags);
                clientDialog.TryOpen();
                clientDialog.OnClosed += () => {
                    clientDialog?.Dispose(); clientDialog = null;
                };

            }

            return true;
        }

        private bool HandleLogWriting(IPlayer byPlayer, float secondsUsed)
        {
            if (!CanWriteViolations(byPlayer, out string error))
            {
                (Api as ICoreServerAPI)?.SendIngameError(byPlayer as IServerPlayer, error);
                return false;
            }

            if (secondsUsed < SnitchDownloadTime)
            {
                if (Api.Side == EnumAppSide.Client)
                {
                    byPlayer.Entity.StartAnimation(SnitchesConstants.SNITCH_DOWNLOAD_ANIMATION);
                }
                return true;
            }

            if (Api.Side == EnumAppSide.Server)
            {
                WriteViolationsToMedium(byPlayer);
                MarkDirty();
            }

            return false;
        }

        private bool CanWriteViolations(IPlayer byPlayer, out string errorcode)
        {
            errorcode = string.Empty;

            if (!HasPermission(byPlayer))
            {
                errorcode = "You do not have permission to use this snitch, activity logged!";
                return false;
            }

            var bookSlot = byPlayer.Entity.ActiveHandItemSlot;
            var penSlot = byPlayer.Entity.LeftHandItemSlot;

            if (!(bookSlot?.Itemstack?.Item is ItemBook))
            {
                errorcode = "You need something to write in! Try a book or a piece of parchment!";
                return false;
            }

            if (penSlot?.Itemstack == null || !penSlot.Itemstack.Item.Attributes?["writingTool"]?.AsBool() == true)
            {
                errorcode = "You need something to write with in your offhand! Try an inkquill!";
                return false;
            }

            return true;
        }

        private void WriteViolationsToMedium(IPlayer byPlayer)
        {
            snitchPlayer = new SnitchPlayer
            {
                playerName = "Snitch_" + Pos.ToLocalPosition(Api),
                playerUID = "Snitch_" + Pos.ToLocalPosition(Api),
                entityPlayer = byPlayer.Entity
            };

            var bookSlot = byPlayer.Entity.ActiveHandItemSlot;
            bookMod.BeginEdit(snitchPlayer, bookSlot);

            string title = "Violations pulled on " + Api.World.Calendar.PrettyDate();
            int maxLogSize = bookSlot.Itemstack.Collectible.Code.ToString().Contains("parchment") ? MaxPaperLog : MaxBookLog;
            var log = violationLogger.GetViolations(maxLogSize, this);

            StringBuilder text = new StringBuilder();
            int writtenCount = 0;
            while (log.Count > 0 && writtenCount < maxLogSize)
            {
                text.AppendLine(log.Dequeue().LogbookFormat(Api));
                writtenCount++;
            }

            bookMod.EndEdit(snitchPlayer, text.ToString(), title, true);
        }

        private void OnPingPlayers(float dt)
        {
            playersPinged.Clear();
            var players = Api.World.GetPlayersAround(Pos.ToVec3d(), Radius, VertRange, ShouldPingPlayer);

            foreach (var player in players)
            {
                TrackPlayer(player);
                playersPinged.Add(player.PlayerUID);
            }

            foreach (var playerUID in playersTracked.ToList())
            {
                if (!playersPinged.Contains(playerUID))
                {
                    UntrackPlayer(playerUID);
                }
            }

            if (Api.Side == EnumAppSide.Server)
            {
                MarkDirty();
            }
        }

        private void TrackPlayer(IPlayer player)
        {
            if (!snitchMod.trackedPlayers.TryGetValue(player.PlayerUID, out var snitches))
            {
                snitches = new List<BlockEntitySnitch>();
                snitchMod.trackedPlayers[player.PlayerUID] = snitches;
            }

            if (!snitches.Contains(this))
            {
                snitches.Add(this);
            }

            if (!playersTracked.Contains(player.PlayerUID))
            {
                playersTracked.Add(player.PlayerUID);
                if (Api.Side == EnumAppSide.Server)
                {
                    AddViolation(new SnitchViolation(EnumViolationType.Trespassed, player as IServerPlayer, player.Entity.Pos.AsBlockPos, Api.World.Calendar.PrettyDate(), Api.World.Calendar.ElapsedDays, Api.World.Calendar.ElapsedSeconds, Api.World.Calendar.Year));
                }
            }
        }

        private void UntrackPlayer(string playerUID)
        {
            if (snitchMod.trackedPlayers.TryGetValue(playerUID, out var snitches))
            {
                var player = Api.World.PlayerByUid(playerUID);
                AddViolation(new SnitchViolation(EnumViolationType.Escaped, player as IServerPlayer, player.Entity.Pos.AsBlockPos, Api.World.Calendar.PrettyDate(), Api.World.Calendar.ElapsedDays, Api.World.Calendar.ElapsedSeconds, Api.World.Calendar.Year));
                snitches.Remove(this);
            }

            playersTracked.Remove(playerUID);
        }

        private bool ShouldPingPlayer(IPlayer player)
        {
            if (!IsOwner(player)) return false;
            if (Sneakable && player.Entity.Controls.Sneak && Pos.DistanceTo(player.Entity.Pos.AsBlockPos) > TrueSightRange) return false;
            return true;
        }

        

        private bool TryActivate()
        {
            if (OnPlayerEnterListenerID == null)
            {
                OnPlayerEnterListenerID = RegisterGameTickListener(OnPingPlayers, 500);
            }

            if (Activated) return false;
            Activated = true;
            MarkDirty();
            return true;
        }

        public void AddViolation(SnitchViolation violation)
        {
            // Check if the violation type is enabled
            if (!IsViolationTypeEnabled(violation.Type)) return;
            violationLogger.AddViolation(violation, this);
        }

        /// <summary>
        /// Enables or disables specific violation types using bitwise flags.
        /// </summary>
        public void SetEnabledViolationFlags(EnumViolationType flags)
        {
            enabledViolationFlags = flags;
        }

        /// <summary>
        /// Checks whether the given violation type is currently enabled.
        /// </summary>
        private bool IsViolationTypeEnabled(EnumViolationType type)
        {
            return (enabledViolationFlags & type) == type;
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

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            RemoveSnitchesFromTracker();
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            if (!HasPermission(forPlayer)) return;

            if (Activated)
            {
                dsc.AppendLine("This Snitch is Activated!");
                dsc.AppendLine(!Sneakable
                    ? $"Radius: {Radius}   Vertical Range: {VertRange}"
                    : $"Radius: {TrueSightRange} / {Radius}   Vertical Range: {VertRange}");
            }

            dsc.AppendLine("Snitch_" + Pos.ToLocalPosition(Api));
            foreach (string player in playersTracked)
            {
                var name = Api.World.PlayerByUid(player)?.PlayerName;
                if (name != null)
                {
                    dsc.AppendLine($"{name} currently being tracked!");
                }
            }
            dsc.AppendLine($"Current Violations: {violationCount} / {MaxSnitchLog}");
        }

        private void RemoveSnitchesFromTracker()
        {
            foreach (var snitchList in snitchMod.trackedPlayers.Values)
            {
                snitchList.Remove(this);
            }
        }

        private bool HasPermission(IPlayer player)
        {
            if (IsOwner(player)) return true;
            return reinforceMod.IsReinforced(Pos) && player.GetGroup(reinforceMod.GetReinforcment(Pos).GroupUid) != null;
        }

        private bool IsOwner(IPlayer player)
        {
            return player.PlayerUID == CurrentOwnerUID;
        }

        public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetid, byte[] data)
        {
            base.OnReceivedClientPacket(fromPlayer, packetid, data);

            if (packetid == 42) { 
                var packet = SerializerUtil.Deserialize<SnitchFlagUpdatePacket>(data);                                
                enabledViolationFlags = packet.Flags;
                
            }
        }
    }


}
