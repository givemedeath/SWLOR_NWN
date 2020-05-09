using System;
using SWLOR.Game.Server.GameObject;

using NWN;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Event.Module;
using SWLOR.Game.Server.Messaging;
using SWLOR.Game.Server.ValueObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Service
{
    public static class HelmetToggleService
    {
        public static void SubscribeEvents()
        {
            MessageHub.Instance.Subscribe<OnModuleEquipItem>(message => OnModuleEquipItem());
            MessageHub.Instance.Subscribe<OnModuleUnequipItem>(message => OnModuleUnequipItem());
        }
        
        private static void OnModuleEquipItem()
        {
            NWPlayer player = (NWScript.GetPCItemLastEquippedBy());
            if (player.GetLocalInt("IS_CUSTOMIZING_ITEM") == NWScript.TRUE) return; // Don't run heavy code when customizing equipment.

            if (!player.IsPlayer || !player.IsInitializedAsPlayer) return;

            NWItem item = (NWScript.GetPCItemLastEquipped());
            if (item.BaseItemType != NWScript.BASE_ITEM_HELMET) return;

            Player pc = DataService.Player.GetByID(player.GlobalID);
            NWScript.SetHiddenWhenEquipped(item.Object, !pc.DisplayHelmet == false ? 0 : 1);
        
        }

        private static void OnModuleUnequipItem()
        {
            NWPlayer player = (NWScript.GetPCItemLastUnequippedBy());

            if (player.GetLocalInt("IS_CUSTOMIZING_ITEM") == NWScript.TRUE) return; // Don't run heavy code when customizing equipment.
            if (!player.IsPlayer) return;

            NWItem item = (NWScript.GetPCItemLastUnequipped());
            if (item.BaseItemType != NWScript.BASE_ITEM_HELMET) return;

            Player pc = DataService.Player.GetByID(player.GlobalID);
            NWScript.SetHiddenWhenEquipped(item.Object, !pc.DisplayHelmet == false ? 0 : 1);
        
        }

        public static void ToggleHelmetDisplay(NWPlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            if (!player.IsPlayer) return;

            Player pc = DataService.Player.GetByID(player.GlobalID);
            pc.DisplayHelmet = !pc.DisplayHelmet;
            DataService.SubmitDataChange(pc, DatabaseActionType.Update);
            
            NWScript.FloatingTextStringOnCreature(
                pc.DisplayHelmet ? "Now showing equipped helmet." : "Now hiding equipped helmet.", 
                player.Object,
                NWScript.FALSE);

            NWItem helmet = (NWScript.GetItemInSlot(NWScript.INVENTORY_SLOT_HEAD, player.Object));
            if (helmet.IsValid)
            {
                NWScript.SetHiddenWhenEquipped(helmet.Object, !pc.DisplayHelmet == false ? 0 : 1);
            }

        }
    }
}
