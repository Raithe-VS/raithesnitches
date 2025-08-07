using ProtoBuf;
using raithesnitches.src.Violations;
using System;
using System.Collections;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;


namespace raithesnitches.src.GUI
{   

    [ProtoContract]
    public class SnitchFlagUpdatePacket
    {
        [ProtoMember(1)] public BlockPos SnitchPos { get; set; }
        [ProtoMember(2)] public EnumViolationType Flags { get; set; }
    }

    [ProtoContract]
    public class SnitchPlayerIgnoreListPacket
    {
        [ProtoMember(1)] public string PlayerName { get; set; }        
    }

    [ProtoContract]
    public class SnitchGroupIgnoreListPacket
    {
        [ProtoMember(1)] public string GroupName { get; set; }
    }

    public class GuiSnitch : GuiDialogBlockEntity
    {
        EnumViolationType currentFlags;        
        BlockPos snitchPos;
        string currentInputAddPlayer;
        string currentInputAddGroup;
        List<string> IgnoredPlayerList = new();
        List<string> IgnoredGroupsList = new();

        Dictionary<EnumViolationType, ElementBounds> toggleBounds = new();
        Dictionary<EnumViolationType, bool> toggled = new();

        public GuiSnitch(BlockPos blockEntityPos, ICoreClientAPI capi, EnumViolationType flags, List<string> ignoredPlayers, List<string> ignoredGroups) : base("SnitchConfig", blockEntityPos, capi)
        {
            this.snitchPos = blockEntityPos;
            this.capi = capi;
            this.currentFlags = flags;
            IgnoredPlayerList = ignoredPlayers; 
            IgnoredGroupsList = ignoredGroups;

            ComposeDialog();            
        }

        void ComposeDialog()
        {
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            bgBounds.BothSizing = ElementSizing.FitToChildren;  


            if (SingleComposer != null) SingleComposer.Dispose();
            SingleComposer = capi.Gui.CreateCompo("snitch-config" + snitchPos, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Snitch Violation Flags & Ignore Lists", OnTitleBarClose)
                .BeginChildElements(bgBounds);

            int i = 0;
            int x = 0;
            int y = 0;

            foreach (EnumViolationType flag in Enum.GetValues(typeof(EnumViolationType)))
            {
                x = i % 2 == 0 ? 0 : 250;
                y = i % 2 == 1 ? -1 : 0;
                
                ElementBounds toggle = ElementBounds.Fixed(0 + x, 40 + (i + y) * 20, 20, 20);
                ElementBounds label = ElementBounds.Fixed(40 + x, 40 + (i + y) * 20, 180, 20);

                toggled[flag] = (currentFlags & flag) == flag;
                toggleBounds[flag] = toggle;

                SingleComposer
                    .AddSwitch((val) => toggled[flag] = val, toggle, $"flag-{flag}")                    
                    .AddStaticText(flag.ToString(), CairoFont.WhiteSmallText(), label);

                SingleComposer.GetSwitch($"flag-{flag}").SetValue(toggled[flag]);    

                i++;
            }

            int baseY = 60 + (i + y) * 20;

            ElementBounds textBounds = ElementBounds.Fixed(0, baseY, 200, 25);
            ElementBounds textInputBoxBounds = ElementBounds.Fixed(0, baseY + 30, 160, 25);
            ElementBounds addButtonBounds = ElementBounds.Fixed(180, baseY + 30, 20, 20);
            ElementBounds removeButtonBounds = ElementBounds.Fixed(200, baseY + 30, 20, 20);
            ElementBounds listBounds = ElementBounds.Fixed(0, baseY + 60, 260, 100);

            SingleComposer
                .AddStaticText("Player Ignore List", CairoFont.WhiteSmallishText(), textBounds)
                .AddTextInput(textInputBoxBounds, OnPlayerTextChanged, CairoFont.WhiteSmallText(), "ignore-player")
                .AddSmallButton("+", OnAddIgnoredPlayer, addButtonBounds)
                .AddSmallButton("-", OnRemovePlayer, removeButtonBounds);

            int offset = 0;
            foreach (string player in IgnoredPlayerList)
            {
                ElementBounds entryBounds = ElementBounds.Fixed(0, baseY + 60 + offset * 20, 250, 20);
                SingleComposer.AddStaticText(player, CairoFont.WhiteSmallText(), entryBounds);
                offset++;
            }

            int baseX = 230;

            ElementBounds groupTextBounds = ElementBounds.Fixed(baseX, baseY, 200, 25);
            ElementBounds groupTextInputBoxBounds = ElementBounds.Fixed(baseX, baseY + 30, 160, 25);
            ElementBounds groupAddButtonBounds = ElementBounds.Fixed(baseX + 180, baseY + 30, 20, 20);
            ElementBounds groupRemoveButtonBounds = ElementBounds.Fixed(baseX + 200, baseY + 30, 20, 20);
            ElementBounds groupListBounds = ElementBounds.Fixed(baseX, baseY + 60, 260, 100);

            SingleComposer
                .AddStaticText("Group Ignore List", CairoFont.WhiteSmallishText(), groupTextBounds)
                .AddTextInput(groupTextInputBoxBounds, OnGroupTextChanged, CairoFont.WhiteSmallText(), "ignore-group")
                .AddSmallButton("+", OnAddIgnoredGroup, groupAddButtonBounds)
                .AddSmallButton("-", OnRemoveGroup, groupRemoveButtonBounds);

            offset = 0;
            foreach (string group in IgnoredGroupsList)
            {
                ElementBounds entryBounds = ElementBounds.Fixed(baseX, baseY + 60 + offset * 20, 250, 20);
                SingleComposer.AddStaticText(group, CairoFont.WhiteSmallText(), entryBounds);
                offset++;
            }


            SingleComposer.EndChildElements();
            SingleComposer.Compose();
        }

        private bool OnRemoveGroup()
        {
            SendSnitchRemoveIgnoreGroupPacket(new SnitchGroupIgnoreListPacket
            {
                GroupName = currentInputAddGroup
            });

            TryClose();
            
            return true;
        }

        private bool OnAddIgnoredGroup()
        {
            SendSnitchAddIgnoreGroupPacket(new SnitchGroupIgnoreListPacket
            {
                GroupName = currentInputAddGroup
            });

            TryClose();           

            return true;
        }

        private void OnGroupTextChanged(string obj)
        {
            currentInputAddGroup = obj;
        }

        private bool OnRemovePlayer()
        {
            SendSnitchRemoveIgnorePlayerPacket(new SnitchPlayerIgnoreListPacket
            {
                PlayerName = currentInputAddPlayer
            });            

            TryClose();

            return true;
        }

        private bool OnAddIgnoredPlayer()
        {
            SendSnitchAddIgnorePlayerPacket(new SnitchPlayerIgnoreListPacket {
                PlayerName = currentInputAddPlayer
            });
            
            TryClose();

            return true;
        }

        private void OnPlayerTextChanged(string obj)
        {
            currentInputAddPlayer = obj;
        }

        private bool OnSave()
        {
            EnumViolationType flags = 0;
            foreach (var kv in toggled)
            {
                if (kv.Value) {
                    flags |= kv.Key;
                }
            }

            SendSnitchFlagUpdatePacket(new SnitchFlagUpdatePacket
            {
                SnitchPos = snitchPos,
                Flags = flags
            });           

            //TryClose();
            return true;
        }

        public override void OnGuiClosed()
        {
            OnSave();
            base.OnGuiClosed();
        }

        private void OnTitleBarClose()
        {            
            TryClose();
        }

        private void SendSnitchFlagUpdatePacket(SnitchFlagUpdatePacket packet)
        {
            capi.Network.SendBlockEntityPacket(snitchPos, 42, SerializerUtil.Serialize(packet));
        }

        private void SendSnitchAddIgnorePlayerPacket(SnitchPlayerIgnoreListPacket packet)
        {
            capi.Network.SendBlockEntityPacket(snitchPos, 43, SerializerUtil.Serialize(packet));
        }
        private void SendSnitchRemoveIgnorePlayerPacket(SnitchPlayerIgnoreListPacket packet)
        {
            capi.Network.SendBlockEntityPacket(snitchPos, 44, SerializerUtil.Serialize(packet));
        }

        private void SendSnitchAddIgnoreGroupPacket(SnitchGroupIgnoreListPacket packet)
        {
            capi.Network.SendBlockEntityPacket(snitchPos, 45, SerializerUtil.Serialize(packet));
        }

        private void SendSnitchRemoveIgnoreGroupPacket(SnitchGroupIgnoreListPacket packet)
        {
            capi.Network.SendBlockEntityPacket(snitchPos, 46, SerializerUtil.Serialize(packet));
        }
    }



}
