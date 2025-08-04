using ProtoBuf;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace raithesnitches.src.Violations
{
    public static class ViolationSeverity
    {
        private static readonly HashSet<EnumViolationType> HighPriorityTypes = new()
    {
        EnumViolationType.BlockBroke,
        EnumViolationType.BlockPlaced,        
        EnumViolationType.ReinforcementPlaced,
        EnumViolationType.EntityKilled,       
        EnumViolationType.EntitySpawned,
        EnumViolationType.CollectibleTaken
    };

        public static bool IsHighPriority(EnumViolationType type) => HighPriorityTypes.Contains(type);
    }

    [ProtoContract]
    public class SnitchViolation
    {
        
        [ProtoMember(1)]
        public string PrettyDate { get; set; }
        [ProtoMember(2)]
        public double Day { get; set; }
        [ProtoMember(3)]
        public long Time { get; set; }
        [ProtoMember(4)]
        public int Year { get; set; }
        [ProtoMember(5)]
        public EnumViolationType Type { get; set; }
        [ProtoMember(6)]        
        public string playerUID { get; set; }
        [ProtoMember(7)]
        public BlockPos position { get; set; }
        [ProtoMember(8)]
        public int BlockID { get; set; }
        [ProtoMember(9)]
        public string BlockName { get; set; }
        [ProtoMember(10)]
        public string EntityName { get; set; }
        [ProtoMember(11)]
        public string CollectibleName { get; set; }
        [ProtoMember(12)]
        public int quantity { get; set; }

        
        public SnitchViolation()
        {

        }

        public SnitchViolation(EnumViolationType type, string playerUID, BlockPos pos, string prettyDate, double day, long time, int year, string blockName = null, string entityName = null, string collectibleName = null, int quantity = 0)
        {
            Type = type;
            this.playerUID = playerUID;
            this.position = pos;
            this.BlockName = blockName;
            this.EntityName = entityName;
            this.CollectibleName = collectibleName;
            this.PrettyDate = prettyDate;
            this.Day = day;
            this.Time = time;
            this.Year = year;
            this.quantity = quantity;
        }

        public SnitchViolation(EnumViolationType type, IServerPlayer player, BlockPos pos, string prettyDate, double day, long time, int year, Block block = null, Entity entity = null, CollectibleObject colObj = null, int quantity = 0)
            : this(type, player.PlayerUID, pos, prettyDate, day, time, year, block?.GetPlacedBlockName(player.Entity.World, pos), entity?.GetName(), colObj?.Code.GetName(), quantity)
        { 
        
        }        

        public SnitchViolation(EnumViolationType type, EntityPlayer player, BlockPos pos, string prettyDate, double day, long time, int year, Block block = null, Entity entity = null, CollectibleObject colObj = null, int quantity = 0)
            : this(type, player.PlayerUID, pos, prettyDate, day, time, year, block?.GetPlacedBlockName(player.World, pos), entity?.GetName(), colObj?.Code.GetName(), quantity)
        {

        }

        public string LogbookFormat(ICoreAPI api)
        {			
			string logbookEntry = $"{PrettyDate} - {api.World.PlayerByUid(playerUID).PlayerName} {GetViolationText()} at {position.ToLocalPosition(api)}";

			return logbookEntry; 
        }

        private string GetViolationText() => Type switch
        {
                EnumViolationType.Trespassed => "tresspassed",
                EnumViolationType.Escaped => "escaped",
                EnumViolationType.BlockUsed => $"used {BlockName ?? "unknown block"}",
                EnumViolationType.BlockPlaced => $"placed {BlockName ?? "unknown block"}",
                EnumViolationType.BlockBroke => $"broke {BlockName ?? "unknown block"}",
			    EnumViolationType.ReinforcementBroke => "reinforcement broken",
				EnumViolationType.ReinforcementPlaced => $"placed reinforcement on {BlockName ?? "unknown block"}",
			    EnumViolationType.EntityInteracted => $"interacted with {EntityName ?? "unknown entity"}",
				EnumViolationType.EntityHit => $"hit {EntityName ?? "unknown entity"}",
				EnumViolationType.EntityKilled => $"killed {EntityName ?? "unknown entity"}",
                EnumViolationType.EntitySpawned => $"spawned {EntityName ?? "unknown entity"}",
				EnumViolationType.CollectibleTaken => $"took {(quantity > 0 ? quantity.ToString() : "")} {CollectibleName ?? "unknown collectible"}",
                EnumViolationType.CollectiblePickedUp => $"picked up {CollectibleName ?? "unknown collectible"}",
				EnumViolationType.CollectibleDropped => $"dropped {CollectibleName ?? "unknown collectible"}",
				_ => "default violation"
        };            
            
            
        
    }
}
