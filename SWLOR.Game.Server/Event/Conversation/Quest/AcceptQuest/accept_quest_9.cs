﻿using SWLOR.Game.Server;
using SWLOR.Game.Server.Event.Conversation.Quest.AcceptQuest;
using static SWLOR.Game.Server.NWN.NWScript;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class accept_quest_9
#pragma warning restore IDE1006 // Naming Styles
    {
        public static int Main()
        {
            return QuestAccept.Check(9) ? 1 : 0;
        }
    }
}
