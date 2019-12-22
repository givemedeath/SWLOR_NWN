﻿using System;
using NWN;
using SWLOR.Game.Server.DoorRule.Contracts;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWScript.Enumerations;

namespace SWLOR.Game.Server.DoorRule
{
    public class Starship2Rule: IDoorRule
    {
        public NWPlaceable Run(NWArea area, Location location, float orientationOverride = 0f, float sqrtValue = 0f)
        {
            float orientationAdjustment = orientationOverride != 0f ? orientationOverride : 270.0f;
            float sqrtAdjustment = sqrtValue != 0f ? sqrtValue : 1.0f;

            Vector position = _.GetPositionFromLocation(location);
            float orientation = _.GetFacingFromLocation(location);

            orientation = orientation + orientationAdjustment;
            if (orientation > 360.0) orientation = orientation - 360.0f;

            float mod = (float)(Math.Sqrt(sqrtAdjustment) * Math.Sin(orientation));
            position.X = position.X + mod;

            mod = (float)(Math.Sqrt(sqrtAdjustment) * Math.Cos(orientation));
            position.Y = position.Y - mod;
            Location doorLocation = _.Location(area.Object, position, _.GetFacingFromLocation(location));

            return _.CreateObject(ObjectType.Placeable, "building_ent1", doorLocation);
        }
    }
}
