using raithesnitches.src.BlockEntities;
using raithesnitches.src.Blocks;
using raithesnitches.src.Config;
using raithesnitches.src.EntityBehaviors;
using raithesnitches.src.Events;
using raithesnitches.src.Violations;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
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
            //config = new SnitchesServerConfig(api);
            //config.Load();
            //config.Save();

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

			tree.SetInt("snitchRadius", config.snitchRadius);
			tree.SetInt("snitchVert", config.snitchVert);			
			tree.SetInt("maxPaperLog", config.maxPaperLog);
			tree.SetInt("maxBookLog", config.maxBookLog);
			tree.SetInt("maxSnitchLog", config.maxSnitchLog);
			tree.SetBool("snitchSneakable", config.snitchSneakable);
			tree.SetFloat("snitchTreusightRange", config.snitchTruesightRange);

		}


		private void GetWorldConfigValues(ICoreClientAPI capi)
		{
			config = new SnitchesServerConfig(capi);
			ITreeAttribute tree = capi.World.Config.GetOrAddTreeAttribute(CONFIG_TREE_DATA_NAME);

			config.snitchRadius = tree.GetInt("snitchRadius", config.snitchRadius);
			config.snitchVert = tree.GetInt("snitchVert", config.snitchVert);			
			config.maxPaperLog = tree.GetInt("maxPaperLog", config.maxPaperLog);
			config.maxBookLog = tree.GetInt("maxBookLog", config.maxBookLog);
			config.maxSnitchLog = tree.GetInt("maxSnitchLog", config.maxSnitchLog);
			config.snitchSneakable = tree.GetBool("snitchSneakable", config.snitchSneakable);
			config.snitchTruesightRange = tree.GetFloat("snitchTruesightRange", config.snitchTruesightRange);
		}
	}
}
