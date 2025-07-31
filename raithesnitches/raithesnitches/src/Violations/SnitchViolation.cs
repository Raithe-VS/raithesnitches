using ProtoBuf;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace raithesnitches.src.Violations
{
    [ProtoContract]
    public class SnitchViolation
    {
        ICoreAPI api;
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
        
        public SnitchViolation()
        {

        }

        public SnitchViolation(EnumViolationType type, IServerPlayer player, BlockPos pos, string prettyDate, double day, long time, int year, Block block = null, Entity entity = null, CollectibleObject colObj = null)
        {
            api = player.Entity.Api;

            this.Type = type;            
            this.playerUID = player.PlayerUID;
            this.position = pos;
            BlockName = block?.GetPlacedBlockName(player.Entity.World, pos);
            //EntityName = entity?.Code.Domain + ":item-creature-" + entity?.Code.Path;
            EntityName = entity?.GetName();
            CollectibleName = colObj?.Code.GetName();

            this.PrettyDate = prettyDate;
            this.Day = day;
            this.Time = time;
            this.Year = year;
        }

        public SnitchViolation(EnumViolationType type, EntityPlayer player, BlockPos pos, string prettyDate, double day, long time, int year, Block block = null, Entity entity = null, CollectibleObject colObj = null)
        {
            api = player.Api;

            this.Type = type;            
            this.playerUID = player.PlayerUID;
            this.position = pos;
            BlockName = block?.GetPlacedBlockName(player.World, pos);
			EntityName = entity?.GetName();
			//EntityName = entity?.Code.Domain + ":item-creature-" + entity?.Code.Path;
            CollectibleName = colObj?.Code.GetName();

            this.PrettyDate = prettyDate;
            this.Day = day;
            this.Time = time;
            this.Year = year;
        }



        public string LogbookFormat(ICoreAPI api)
        {
			//string logbookEntry = $"At {this.Time} - {api.World.PlayerByUid(playerUID).PlayerName} {GetViolationText()} at {position.ToLocalPosition(api)}";
			string logbookEntry = $"{PrettyDate} - {api.World.PlayerByUid(playerUID).PlayerName} {GetViolationText()} at {position.ToLocalPosition(api)}";

			return logbookEntry; 
        }

        private string GetViolationText() => Type switch
        {
                EnumViolationType.Trespassed => "tresspassed",
                EnumViolationType.Escaped => "escaped",
                EnumViolationType.BlockUsed => "used " + BlockName,
                EnumViolationType.BlockPlaced => "placed " + BlockName,
                EnumViolationType.BlockBroke => "broke " + BlockName,
			    EnumViolationType.ReinforcementBroke => "reinforcement broken",
				EnumViolationType.ReinforcementPlaced => "reinforcement placed",
			    EnumViolationType.EntityInteracted => "interacted with " + EntityName,
				EnumViolationType.EntityHit => "hit " + EntityName,
				EnumViolationType.EntityKilled => "killed " + EntityName,
				EnumViolationType.EntitySpawned => "spawn " + EntityName,
				EnumViolationType.CollectibleTaken => "taken " + CollectibleName,
			    EnumViolationType.CollectiblePickedUp => "picked up " + CollectibleName,
				EnumViolationType.CollectibleDropped => "dropped " + CollectibleName,
				_ => "default violation"
        };            
            
            
        
    }
}
