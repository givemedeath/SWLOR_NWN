﻿using System.Linq;
using NWN;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.OverflowStorage
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
            NWPlaceable container = (NWScript.OBJECT_SELF);
            NWPlayer oPC = (NWScript.GetLastOpenedBy());
            var items = DataService.PCOverflowItem.GetAllByPlayerID(oPC.GlobalID);
            foreach (PCOverflowItem item in items)
            {
                NWItem oItem = SerializationService.DeserializeItem(item.ItemObject, container);
                oItem.SetLocalString("TEMP_OVERFLOW_ITEM_ID", item.ID.ToString());
            }

            container.IsUseable = false;
        }
    }
}
