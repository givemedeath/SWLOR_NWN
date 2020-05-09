﻿using NWN;
using SWLOR.Game.Server.GameObject;


using SWLOR.Game.Server.ValueObject.Dialog;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Conversation
{
    public class DestroySurvivalKnife: ConversationBase
    {
        public override PlayerDialog SetUp(NWPlayer player)
        {
            PlayerDialog dialog = new PlayerDialog("MainPage");
            DialogPage mainPage = new DialogPage(
                "Are you sure you want to destroy your survival knife? This action is irreversible!",
                "Destroy Survival Knife"
            );

            dialog.AddPage("MainPage", mainPage);
            return dialog;
        }

        public override void Initialize()
        {
        }

        public override void DoAction(NWPlayer player, string pageName, int responseID)
        {
            switch (responseID)
            {
                case 1:
                    NWItem item = (NWScript.GetItemPossessedBy(player.Object, "survival_knife"));
                    NWScript.DestroyObject(item.Object);
                    EndConversation();
                    break;
            }
        }

        public override void Back(NWPlayer player, string beforeMovePage, string afterMovePage)
        {
        }

        public override void EndDialog()
        {
        }
    }
}
