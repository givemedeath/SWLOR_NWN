﻿using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Quest;
using SWLOR.Game.Server.Service;

namespace SWLOR.Game.Server.Scripting.Quest
{
    public class CZ220SuppliesEngineering: AbstractQuest
    {
        public CZ220SuppliesEngineering()
        {
            CreateQuest(4, "CZ-220 Supplies - Engineering", "cz220_engineering")
                
                .AddObjectiveCollectItem(1, "scanner_r_b", 1, true)
                .AddObjectiveTalkToNPC(2)

                .AddRewardGold(50)
                .AddRewardKeyItem(KeyItem.CraftingTerminalDroidOperatorWorkReceipt)
                .AddRewardFame(FameRegion.CZ220, 5)

                .OnAccepted((player, questGiver) =>
                {
                    KeyItemService.GivePlayerKeyItem(player, KeyItem.CraftingTerminalDroidOperatorWorkOrder);
                })
                .OnCompleted((player, questGiver) =>
                {
                    KeyItemService.RemovePlayerKeyItem(player, KeyItem.CraftingTerminalDroidOperatorWorkOrder);
                });
        }
    }
}
