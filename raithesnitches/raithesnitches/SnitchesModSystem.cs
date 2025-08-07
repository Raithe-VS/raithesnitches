using raithesnitches.src.BlockEntities;
using raithesnitches.src.Blocks;
using raithesnitches.src.Config;
using raithesnitches.src.Constants;
using raithesnitches.src.EntityBehaviors;
using raithesnitches.src.Events;
using raithesnitches.src.Violations;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;


namespace raithesnitches
{
    public class SnitchesModSystem : ModSystem
    {
        ICoreAPI _api;

		public const string CONFIG_TREE_DATA_NAME = "SnitchesConfig";
		public const string CONFIG_FOLDER_NAME = "RaitheSnitches/";
		public const string SERVER_CONFIG_FILE_NAME = "snitches_server.json";        

		public static Dictionary<string, SnitchesConfig> Configs;

		public ViolationLogger violationLogger;

        public Dictionary<string, List<BlockEntitySnitch>> trackedPlayers;
		public List<BlockEntitySnitch> loadedSnitches;

        public override void StartPre(ICoreAPI api)
        {
            
            base.StartPre(api);
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockSnitch", typeof(BlockSnitch));
            api.RegisterBlockEntityClass("BESnitch", typeof(BlockEntitySnitch));

            trackedPlayers = new Dictionary<string, List<BlockEntitySnitch>>();

            _api = api;

            base.Start(api);
        }
       
        public override void StartServerSide(ICoreServerAPI api)
        {
			violationLogger = new ViolationLogger(api);
			loadedSnitches = new List<BlockEntitySnitch>();

			api.Event.OnEntityLoaded += AddEntityBehaviors;
			api.Event.OnEntitySpawn += AddEntityBehaviors;

			api.Event.OnEntityDeath += SnitchEventsServer.OnEntityDeath;			
			api.Event.OnEntitySpawn += SnitchEventsServer.OnEntitySpawn;			

            api.Event.DidUseBlock += SnitchEventsServer.DidUseBlock;
            api.Event.DidPlaceBlock += SnitchEventsServer.DidPlaceBlock;
            api.Event.DidBreakBlock += SnitchEventsServer.DidBreakBlock;

			TryLoadConfig(api);
			SetWorldConfigValues(api);

            base.StartServerSide(api);
        }

		public override void StartClientSide(ICoreClientAPI api)
		{
			GetWorldConfigValues(api);

			base.StartClientSide(api);
		}


		private void AddEntityBehaviors(Entity entity)
		{
			if (entity != null && !entity.HasBehavior<EntityBehaviorSnitchOnEntityHitTrigger>())
			{
				entity.AddBehavior(new EntityBehaviorSnitchOnEntityHitTrigger(entity));
			}
		}

        private void TryLoadConfig(ICoreAPI api)
        {
			try
			{
				var configs = api.LoadModConfig<SnitchesServerConfigs>(CONFIG_FOLDER_NAME + SERVER_CONFIG_FILE_NAME);
				
				if (configs == null)
				{
					
					Configs = SnitchesConfig.CreateBaseConfigs(api);
					configs = new SnitchesServerConfigs()
					{
						SnitchConfigs = Configs.Values.ToArray()
					};
				} else
				{
					Configs = new();
					foreach(var config in configs.SnitchConfigs)
					{
						Configs.Add(config.code, config);
					}
				}
				api.StoreModConfig(configs, CONFIG_FOLDER_NAME + SERVER_CONFIG_FILE_NAME);
			}
			catch (Exception e)
			{
				Mod.Logger.Error("Could not load config! Loading defualt values!");
				Mod.Logger.Error(e);
				Configs = SnitchesConfig.CreateBaseConfigs(api);
			}
		}

		private void SetWorldConfigValues(ICoreServerAPI sapi)
		{
			ITreeAttribute configTree = sapi.World.Config.GetOrAddTreeAttribute(CONFIG_TREE_DATA_NAME);

			foreach (var config in Configs.Values) {

				var tree = configTree.GetOrAddTreeAttribute(config.code);

				tree.SetString("code", config.code);
                tree.SetInt(SnitchesConstants.SNITCH_RADIUS, config.snitchRadius);
                tree.SetInt(SnitchesConstants.SNITCH_VERTICAL_DISTANCE, config.snitchVerticalRange);
                tree.SetInt(SnitchesConstants.SNITCH_MAX_PAPER_LOG, config.maxPaperLog);
                tree.SetInt(SnitchesConstants.SNITCH_MAX_BOOK_LOG, config.maxBookLog);
                tree.SetInt(SnitchesConstants.SNITCH_MAX_LOG, config.snitchMaxLog);
                tree.SetBool(SnitchesConstants.SNITCH_SNEAKABLE, config.snitchSneakable);
                tree.SetFloat(SnitchesConstants.SNITCH_TRUESIGHT_RANGE, config.snitchTruesightRange);
                tree.SetFloat(SnitchesConstants.SNITCH_DOWNLOAD_TIME, config.snitchDownloadTime);

            }			

		}


		private void GetWorldConfigValues(ICoreClientAPI capi)
		{			

			Configs = new();
            ITreeAttribute configTree = capi.World.Config.GetOrAddTreeAttribute(CONFIG_TREE_DATA_NAME);

			foreach(var tree in configTree.Values)
			{
				if(tree is ITreeAttribute itree)
				{
					var temp = new SnitchesConfig(capi,
						itree.GetString("code"),
						itree.GetBool(SnitchesConstants.SNITCH_SNEAKABLE),
						itree.GetInt(SnitchesConstants.SNITCH_RADIUS),
						itree.GetInt(SnitchesConstants.SNITCH_VERTICAL_DISTANCE),
						itree.GetFloat(SnitchesConstants.SNITCH_TRUESIGHT_RANGE),
						itree.GetInt(SnitchesConstants.SNITCH_MAX_LOG),
						itree.GetInt(SnitchesConstants.SNITCH_MAX_BOOK_LOG),
						itree.GetInt(SnitchesConstants.SNITCH_MAX_PAPER_LOG),
						itree.GetFloat(SnitchesConstants.SNITCH_DOWNLOAD_TIME));

					Configs.Add(temp.code, temp);
				}
            }
        }
	}
}
