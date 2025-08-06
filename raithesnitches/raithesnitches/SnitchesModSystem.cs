using raithesnitches.src.BlockEntities;
using raithesnitches.src.Blocks;
using raithesnitches.src.Config;
using raithesnitches.src.Constants;
using raithesnitches.src.EntityBehaviors;
using raithesnitches.src.Events;
using raithesnitches.src.Violations;
using System;
using System.Collections.Generic;
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
		

        public static SnitchesServerConfig config;

		public ViolationLogger violationLogger;

        public Dictionary<string, List<BlockEntitySnitch>> trackedPlayers;

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

			api.Event.OnEntityLoaded += AddEntityBehaviors;
			api.Event.OnEntitySpawn += AddEntityBehaviors;

			api.Event.OnEntityDeath += SnitchEventsServer.OnEntityDeath;
			api.Event.OnPlayerInteractEntity += SnitchEventsServer.OnPlayerInteractEntity;
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
				config = api.LoadModConfig<SnitchesServerConfig>(CONFIG_FOLDER_NAME + SERVER_CONFIG_FILE_NAME);
				if (config == null)
				{
					config = new SnitchesServerConfig(api);
				}
				api.StoreModConfig(config, CONFIG_FOLDER_NAME + SERVER_CONFIG_FILE_NAME);
			}
			catch (Exception e)
			{
				Mod.Logger.Error("Could not load config! Loading defualt values!");
				Mod.Logger.Error(e);
				config = new SnitchesServerConfig(api);
			}
		}

		private void SetWorldConfigValues(ICoreServerAPI sapi)
		{
			ITreeAttribute tree = sapi.World.Config.GetOrAddTreeAttribute(CONFIG_TREE_DATA_NAME);

			tree.SetInt(SnitchesConstants.SNITCH_RADIUS, config.snitchRadius);
			tree.SetInt(SnitchesConstants.SNITCH_VERTICAL_DISTANCE, config.snitchVerticalRange);			
			tree.SetInt(SnitchesConstants.SNITCH_MAX_PAPER_LOG, config.maxPaperLog);
			tree.SetInt(SnitchesConstants.SNITCH_MAX_BOOK_LOG, config.maxBookLog);
			tree.SetInt(SnitchesConstants.SNITCH_MAX_LOG, config.snitchMaxLog);
			tree.SetBool(SnitchesConstants.SNITCH_SNEAKABLE, config.snitchSneakable);
			tree.SetFloat(SnitchesConstants.SNITCH_TRUESIGHT_RANGE, config.snitchTruesightRange);
			tree.SetFloat(SnitchesConstants.SNITCH_DOWNLOAD_TIME, config.snitchDownloadTime);

		}


		private void GetWorldConfigValues(ICoreClientAPI capi)
		{
			config = new SnitchesServerConfig(capi);
			ITreeAttribute tree = capi.World.Config.GetOrAddTreeAttribute(CONFIG_TREE_DATA_NAME);

			config.snitchRadius = tree.GetInt(SnitchesConstants.SNITCH_RADIUS, config.snitchRadius);
			config.snitchVerticalRange = tree.GetInt(SnitchesConstants.SNITCH_VERTICAL_DISTANCE, config.snitchVerticalRange);			
			config.maxPaperLog = tree.GetInt(SnitchesConstants.SNITCH_MAX_PAPER_LOG, config.maxPaperLog);
			config.maxBookLog = tree.GetInt(SnitchesConstants.SNITCH_MAX_BOOK_LOG, config.maxBookLog);
			config.snitchMaxLog = tree.GetInt(SnitchesConstants.SNITCH_MAX_LOG, config.snitchMaxLog);
			config.snitchSneakable = tree.GetBool(SnitchesConstants.SNITCH_SNEAKABLE, config.snitchSneakable);
			config.snitchTruesightRange = tree.GetFloat(SnitchesConstants.SNITCH_TRUESIGHT_RANGE, config.snitchTruesightRange);
			config.snitchDownloadTime = tree.GetFloat(SnitchesConstants.SNITCH_DOWNLOAD_ANIMATION, config.snitchDownloadTime);
		}
	}
}
