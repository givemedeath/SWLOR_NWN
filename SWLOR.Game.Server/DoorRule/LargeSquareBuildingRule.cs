﻿using NWN;
using SWLOR.Game.Server.DoorRule.Contracts;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWN;
using static SWLOR.Game.Server.NWN.NWScript;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.DoorRule
{
    public class LargeSquareBuildingRule: IDoorRule
    {
        public NWPlaceable Run(NWArea area, Location location, float orientationOverride = -1f, float sqrtValue = -1f)
        {
            float orientationAdjustment = orientationOverride != 0f ? orientationOverride : 90f;
            float sqrtAdjustment = sqrtValue != 0f ? sqrtValue : 34f;

            Vector position = NWScript.GetPositionFromLocation(location);
            float orientation = NWScript.GetFacingFromLocation(location);

            orientation = orientation + orientationAdjustment;
            if (orientation > 360.0) orientation = orientation - 360.0f;

            float mod = NWScript.sqrt(sqrtAdjustment) * NWScript.sin(orientation);
            position.X = position.X + mod;

            mod = NWScript.sqrt(sqrtAdjustment) * NWScript.cos(orientation);
            position.Y = position.Y - mod;
            Location doorLocation = NWScript.Location(area.Object, position, NWScript.GetFacingFromLocation(location));

            return NWScript.CreateObject(OBJECT_TYPE_PLACEABLE, "building_ent1", doorLocation);
        }
    }
}
