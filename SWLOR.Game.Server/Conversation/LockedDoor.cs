﻿using System.Collections.Generic;
using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.Service;

using SWLOR.Game.Server.ValueObject.Dialog;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Conversation
{
    public class LockedDoor: ConversationBase
    {
        public override PlayerDialog SetUp(NWPlayer player)
        {
            PlayerDialog dialog = new PlayerDialog("MainPage");

            DialogPage mainPage = new DialogPage(
                "This door is locked. It looks like it needs a key to be opened.",
                "Use Key");

            dialog.AddPage("MainPage", mainPage);
            return dialog;
        }

        public override void Initialize()
        {
            NWObject door = NWScript.OBJECT_SELF;
            NWPlayer player = GetPC();
            List<int> keyItemIDs = new List<int>();

            int count = 1;
            int keyItemID = door.GetLocalInt("REQUIRED_KEY_ITEM_ID_" + count);
            while (keyItemID > 0)
            {
                keyItemIDs.Add(keyItemID);

                count++;
                keyItemID = door.GetLocalInt("REQUIRED_KEY_ITEM_ID_" + count);
            }
            
            bool hasKeyItems = KeyItemService.PlayerHasAllKeyItems(player, keyItemIDs.ToArray());
            string doorDialogue = door.GetLocalString("DOOR_DIALOGUE");

            if (!string.IsNullOrWhiteSpace(doorDialogue))
            {
                SetPageHeader("MainPage", doorDialogue);
            }

            if (hasKeyItems)
            {
                SetResponseText("MainPage", 1, "Open Door");
            }
            else
            {
                SetResponseVisible("MainPage", 1, false);
            }
        }

        public override void DoAction(NWPlayer player, string pageName, int responseID)
        {
            if (responseID != 1) return;

            NWObject door = NWScript.OBJECT_SELF;
            string insideWP = door.GetLocalString("LOCKED_DOOR_INSIDE_WP");
            NWObject wp = NWScript.GetWaypointByTag(insideWP);
            Location portTo = wp.Location;

            NWScript.AssignCommand(player, () =>
            {
                NWScript.ActionJumpToLocation(portTo);
            });

            EndConversation();
        }

        public override void Back(NWPlayer player, string beforeMovePage, string afterMovePage)
        {
        }

        public override void EndDialog()
        {
        }
    }
}
