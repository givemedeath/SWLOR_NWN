﻿using SWLOR.Game.Server.Core.NWScript;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;

namespace SWLOR.Game.Server.ChatCommand
{
    [CommandDetails("Plays a cross arms animation.", CommandPermissionType.Player | CommandPermissionType.DM | CommandPermissionType.Admin)]
    public class CrossArms : LoopingAnimationCommand
    {
        protected override void DoAction(NWPlayer user, float duration)
        {
            user.AssignCommand(() =>
            {
                NWScript.ActionPlayAnimation(Animation.LoopingCustom4, 1.0f, duration);
            });
        }
    }
}