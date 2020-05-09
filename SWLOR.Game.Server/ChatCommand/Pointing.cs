﻿using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using static SWLOR.Game.Server.NWN.NWScript;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.ChatCommand
{
    [CommandDetails("Plays a pointing animation.", CommandPermissionType.Player | CommandPermissionType.DM | CommandPermissionType.Admin)]
    public class Pointing : LoopingAnimationCommand
    {
        protected override void DoAction(NWPlayer user, float duration)
        {
            user.AssignCommand(() =>
            {
                NWScript.ActionPlayAnimation(ANIMATION_LOOPING_CUSTOM1, 1.0f, duration);
            });
        }
    }
}