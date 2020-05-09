﻿using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.MarketTerminal
{
    public class OnUsed: IScript
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
            NWPlaceable device = NWScript.OBJECT_SELF;

            if (player.IsBusy)
            {
                player.SendMessage("You are too busy to do that right now.");
                return;
            }

            DialogService.StartConversation(player, device, "MarketTerminal");
        }
    }
}
