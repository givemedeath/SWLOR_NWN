﻿using System.Collections.Generic;
using System.Linq;
using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.CraftingDevice
{
    public class OnOpened: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable device = (NWScript.OBJECT_SELF);
            NWPlayer oPC = (NWScript.GetLastOpenedBy());
            var model = CraftService.GetPlayerCraftingData(oPC);
            
            if (model.Access != CraftingAccessType.None)
            {
                NWItem menuItem = (NWScript.CreateItemOnObject("cft_confirm", device.Object));
                NWPlaceable storage = (NWScript.GetObjectByTag("craft_temp_store"));
                var storageItems = storage.InventoryItems.ToList();
                List<NWItem> list = null;

                if (model.Access == CraftingAccessType.MainComponent)
                {
                    menuItem.Name = "Confirm Main Components";
                    list = model.MainComponents;
                }
                else if (model.Access == CraftingAccessType.SecondaryComponent)
                {
                    menuItem.Name = "Confirm Secondary Components";
                    list = model.SecondaryComponents;
                }
                else if (model.Access == CraftingAccessType.TertiaryComponent)
                {
                    menuItem.Name = "Confirm Tertiary Components";
                    list = model.TertiaryComponents;
                }
                else if (model.Access == CraftingAccessType.Enhancement)
                {
                    menuItem.Name = "Confirm Enhancement Components";
                    list = model.EnhancementComponents;
                }

                if (list == null)
                {
                    oPC.FloatingText("Error locating component list. Notify an admin.");
                    return;
                }

                foreach (var item in list)
                {
                    NWItem storageItem = storageItems.Single(x => x.GlobalID == item.GlobalID);
                    NWScript.CopyItem(storageItem.Object, device.Object, NWScript.TRUE);
                }

                oPC.FloatingText("Place the components inside the container and then click the item named '" + menuItem.Name + "' to continue.");
            }

            device.IsLocked = true;
            return;
        }


    }
}
