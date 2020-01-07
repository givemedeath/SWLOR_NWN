﻿using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Quest;

namespace SWLOR.Game.Server.Scripting.Quest
{
    public class TheAbandonedStation: AbstractQuest
    {
        public TheAbandonedStation()
        {
            CreateQuest(31, "The Abandoned Station", "aban_station")
                .AddPrerequisiteQuest(29)

                .AddObjectiveKillTarget(1, NPCGroup.AbandonedStationBoss, 1)
                .AddObjectiveTalkToNPC(2)

                .AddRewardGold(4000)
                .AddRewardFame(FameRegion.Global, 20);
        }
    }
}