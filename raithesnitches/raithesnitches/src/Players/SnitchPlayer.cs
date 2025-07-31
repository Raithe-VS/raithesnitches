using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace raithesnitches.src.Players
{
    public class SnitchPlayer : IPlayer
    {
        public IPlayerRole Role { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public PlayerGroupMembership[] Groups => throw new NotImplementedException();

        public List<Entitlement> Entitlements => throw new NotImplementedException();

        public BlockSelection CurrentBlockSelection => throw new NotImplementedException();

        public EntitySelection CurrentEntitySelection => throw new NotImplementedException();

        public string PlayerName => playerName;

        public string playerName;

        public string PlayerUID => playerUID;

        public string playerUID;

        public int ClientId => throw new NotImplementedException();

        public EntityPlayer Entity => entityPlayer;

        public EntityPlayer entityPlayer;

        public IWorldPlayerData WorldData => throw new NotImplementedException();

        public IPlayerInventoryManager InventoryManager => throw new NotImplementedException();

        public string[] Privileges => throw new NotImplementedException();

        public bool ImmersiveFpMode => throw new NotImplementedException();

        public PlayerGroupMembership GetGroup(int groupId)
        {
            throw new NotImplementedException();
        }

        public PlayerGroupMembership[] GetGroups()
        {
            throw new NotImplementedException();
        }

        public bool HasPrivilege(string privilegeCode)
        {
            throw new NotImplementedException();
        }
    }
}
