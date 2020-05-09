﻿using SWLOR.Game.Server;

using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.ValueObject;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class warp_to_wp
#pragma warning restore IDE1006 // Naming Styles
    {
        public static void Main()
        {
            using (new Profiler(nameof(warp_to_wp)))
            {
                NWPlayer player = SWLOR.Game.Server.NWN.NWScript.GetPCSpeaker();
                NWObject talkingTo = SWLOR.Game.Server.NWN.NWScript.OBJECT_SELF;

                string waypointTag = talkingTo.GetLocalString("DESTINATION");
                NWObject waypoint = SWLOR.Game.Server.NWN.NWScript.GetWaypointByTag(waypointTag);

                player.AssignCommand(() => { SWLOR.Game.Server.NWN.NWScript.ActionJumpToLocation(waypoint.Location); });
            }
        }
    }
}
