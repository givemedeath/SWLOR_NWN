﻿using System.Linq;
using NWN;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.TutorialPortal
{
    public class OnUsed: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlayer player = (NWScript.GetLastUsedBy());
            NWPlaceable warp = (NWScript.OBJECT_SELF);
            bool isExit = warp.GetLocalInt("IS_EXIT") == NWScript.TRUE;

            if (isExit)
            {
                Player entity = PlayerService.GetPlayerEntity(player.GlobalID);
                NWArea area = NWModule.Get().Areas.Single(x => x.Resref == entity.LocationAreaResref);
                Vector position = NWScript.Vector((float) entity.LocationX, (float) entity.LocationY, (float) entity.LocationZ);
                Location location = NWScript.Location(area.Object,
                    position,
                    (float) entity.LocationOrientation);

                player.AssignCommand(() => NWScript.ActionJumpToLocation(location));
            }
            else
            {
                PlayerService.SaveLocation(player);
                NWObject waypoint = (NWScript.GetWaypointByTag("TUTORIAL_WP"));
                player.AssignCommand(() => NWScript.ActionJumpToLocation(waypoint.Location));
            }
        }
    }
}
