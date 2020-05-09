﻿using System;
using System.Linq;
using NWN;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.QuestSystem.ItemCollector
{
    public class OnDisturbed : IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable container = NWScript.OBJECT_SELF;
            NWObject owner = container.GetLocalObject("QUEST_OWNER");

            NWPlayer player = NWScript.GetLastDisturbed();
            NWItem item = NWScript.GetInventoryDisturbItem();
            int disturbType = NWScript.GetInventoryDisturbType();
            string crafterPlayerID = item.GetLocalString("CRAFTER_PLAYER_ID");
            Guid? crafterPlayerGUID = null;
            if (!string.IsNullOrWhiteSpace(crafterPlayerID))
                crafterPlayerGUID = new Guid(crafterPlayerID);

            if (disturbType == NWScript.INVENTORY_DISTURB_TYPE_ADDED)
            {
                int questID = container.GetLocalInt("QUEST_ID");
                PCQuestStatus status = DataService.PCQuestStatus.GetByPlayerAndQuestID(player.GlobalID, questID);
                PCQuestItemProgress progress = DataService.PCQuestItemProgress.GetByPCQuestStatusIDAndResrefOrDefault(status.ID, item.Resref);
                DatabaseActionType action = DatabaseActionType.Update;

                if (progress == null)
                {
                    NWScript.CopyItem(item, player, NWScript.TRUE);
                    player.SendMessage(ColorTokenService.Red("That item is not required for this quest."));
                }
                else if (progress.MustBeCraftedByPlayer && crafterPlayerGUID != player.GlobalID)
                {
                    NWScript.CopyItem(item, player, NWScript.TRUE);
                    player.SendMessage(ColorTokenService.Red("You may only submit items which you have personally created for this quest."));
                }
                else
                {
                    progress.Remaining--;

                    if (progress.Remaining <= 0)
                    {
                        var progressCopy = progress;
                        progress = DataService.PCQuestItemProgress.GetByID(progressCopy.ID);
                        action = DatabaseActionType.Delete;
                    }
                    DataService.SubmitDataChange(progress, action);

                    // Recalc the remaining items needed.
                    int remainingCount = DataService.PCQuestItemProgress.GetCountByPCQuestStatusID(status.ID);
                    if (remainingCount <= 0)
                    {
                        var quest = QuestService.GetQuestByID(questID);
                        quest.Advance(player, owner);
                    }

                    player.SendMessage("You need " + progress.Remaining + "x " + item.Name + " for this quest.");
                }
                item.Destroy();

                var questItemProgresses = DataService.PCQuestItemProgress.GetAllByPCQuestStatusID(status.ID);
                if (!questItemProgresses.Any())
                {
                    string conversation = NWScript.GetLocalString(owner, "CONVERSATION");

                    // Either start a SWLOR conversation
                    if (!string.IsNullOrWhiteSpace(conversation))
                    {
                        DialogService.StartConversation(player, owner, conversation);
                    }
                    // Or a regular NWN conversation.
                    else
                    {
                        player.AssignCommand(() => { NWScript.ActionStartConversation(owner, "", NWScript.TRUE, NWScript.FALSE); });
                    }
                }
            }
        }
    }
}
