using raithesnitches.src.BlockEntities;
using raithesnitches.src.Constants;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace raithesnitches.src.Blocks
{
    public class BlockSnitch : Block
    {
        WorldInteraction[] interactions;        

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
			BlockEntitySnitch snitch = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntitySnitch;

            if (snitch != null) { return true; }
            return false;
		}

		public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
		{
			BlockEntitySnitch snitch = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntitySnitch;
            if (snitch == null) return false;
            
			return snitch.OnInteract(byPlayer, secondsUsed);
		}

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            base.OnBlockInteractStop(secondsUsed, world, byPlayer, blockSel);

            if (byPlayer.Entity.AnimManager.IsAnimationActive(SnitchesConstants.SNITCH_DOWNLOAD_ANIMATION))
            {
                byPlayer.Entity.StopAnimation(SnitchesConstants.SNITCH_DOWNLOAD_ANIMATION);
            }
        }

        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, EnumItemUseCancelReason cancelReason)
        {
            if (byPlayer.Entity.AnimManager.IsAnimationActive(SnitchesConstants.SNITCH_DOWNLOAD_ANIMATION))
            {
                byPlayer.Entity.StopAnimation(SnitchesConstants.SNITCH_DOWNLOAD_ANIMATION);
            }

            return base.OnBlockInteractCancel(secondsUsed, world, byPlayer, blockSel, cancelReason);
        }

		public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            List<ItemStack> bookStackList = new List<ItemStack>();

            foreach (Item item in api.World.Items)
            {
                if (item.Code == null) continue;

                if (item is ItemBook)
                {
                    bookStackList.Add(new ItemStack(item));
                }

            }

            interactions = ObjectCacheUtil.GetOrCreate(api, "snitchBlockInteractions", () =>
            {
                return new WorldInteraction[]{
                    new WorldInteraction() {
                        ActionLangCode = "blockhelp-snitch-activate",
                        HotKeyCode = "shift",
                        MouseButton = EnumMouseButton.Right,
                        ShouldApply = (wi, bs, es) => {
                            BlockEntitySnitch bes = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntitySnitch;
                            return bes?.Activated != true;
                        }
                    },                    
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-snitch-writeviolations",
                        HotKeyCode = "ctrl",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = bookStackList.ToArray(),

                        ShouldApply = (wi, bs, es) => {
                            BlockEntitySnitch bes = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntitySnitch;
                            return bes?.Activated == true;
                        }
                    }
                };

            });
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
