﻿using SWLOR.Game.Server;
using SWLOR.Game.Server.Event.Conversation.Quest.HasQuest;
using static SWLOR.Game.Server.NWN.NWScript;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class has_quest_5
#pragma warning restore IDE1006 // Naming Styles
    {
        public static int Main()
        {
            return QuestCheck.Check(5) ? 1 : 0;
        }
    }
}
