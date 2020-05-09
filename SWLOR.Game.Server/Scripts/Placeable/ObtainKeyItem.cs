﻿using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable
{
    public class ObtainKeyItem: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlayer player = NWScript.GetLastUsedBy();

            if (!player.IsPlayer) return;

            NWPlaceable placeable = NWScript.OBJECT_SELF;
            int keyItemID = placeable.GetLocalInt("KEY_ITEM_ID");
            
            if (keyItemID <= 0) return;

            if (KeyItemService.PlayerHasKeyItem(player, keyItemID))
            {
                player.SendMessage("You already have this key item.");
                return;
            }

            KeyItemService.GivePlayerKeyItem(player, keyItemID);

            string visibilityGUID = placeable.GetLocalString("VISIBILITY_OBJECT_ID");
            if (!string.IsNullOrWhiteSpace(visibilityGUID))
            {
                ObjectVisibilityService.AdjustVisibility(player, placeable, false);
            }
        }
    }
}
