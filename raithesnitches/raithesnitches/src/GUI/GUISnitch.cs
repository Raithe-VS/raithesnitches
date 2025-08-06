using ProtoBuf;
using raithesnitches.src.Violations;
using System;
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

    public class GuiSnitch : GuiDialogBlockEntity
    {
        EnumViolationType currentFlags;
        BlockPos snitchPos;        

        Dictionary<EnumViolationType, ElementBounds> toggleBounds = new();
        Dictionary<EnumViolationType, bool> toggled = new();

        public GuiSnitch(BlockPos blockEntityPos, ICoreClientAPI capi, EnumViolationType flags) : base("SnitchConfig", blockEntityPos, capi)
        {
            this.snitchPos = blockEntityPos;
            this.capi = capi;
            this.currentFlags = flags;

            ComposeDialog();
        }

        void ComposeDialog()
        {
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
            bgBounds.BothSizing = ElementSizing.FitToChildren;

            if (SingleComposer != null) SingleComposer.Dispose();
            SingleComposer = capi.Gui.CreateCompo("snitch-config", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Snitch Violation Flags", OnTitleBarClose)
                .BeginChildElements(bgBounds);

            int i = 0;
            
            foreach (EnumViolationType flag in Enum.GetValues(typeof(EnumViolationType)))
            {
                int x = i % 2 == 0 ? 0 : 250;
                int y = i % 2 == 1 ? -1 : 0;
                
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

            ElementBounds saveButton = ElementBounds.Fixed(0, 50 + i * 20, 100, 25);
            SingleComposer.AddSmallButton("Save", OnSave, saveButton);

            SingleComposer.EndChildElements();
            SingleComposer.Compose();
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

            capi.Network.SendBlockEntityPacket(snitchPos, 42, SerializerUtil.Serialize(new SnitchFlagUpdatePacket
            {
                SnitchPos = snitchPos,
                Flags = flags
            }));

            TryClose();
            return true;
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        private void SendSanctuaryPacket(SnitchFlagUpdatePacket packet)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition, 37, SerializerUtil.Serialize(packet));
        }
    }



}
