﻿using NWN;
using SWLOR.Game.Server.NWN;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.GameObject
{
    public class NWLocation
    {
        public Location Location { get; set; }

        public NWLocation(Location nwnLocation)
        {
            Location = nwnLocation;
        }

        public float X => NWScript.GetPositionFromLocation(Location).X;
        public float Y => NWScript.GetPositionFromLocation(Location).Y;
        public float Z => NWScript.GetPositionFromLocation(Location).Z;
        public float Orientation => NWScript.GetFacingFromLocation(Location);

        public NWArea Area => NWScript.GetAreaFromLocation(Location);

        public Vector Position => NWScript.GetPositionFromLocation(Location);

        public static implicit operator Location(NWLocation l)
        {
            return l.Location;
        }

        public static implicit operator NWLocation(Location l)
        {
            return new NWLocation(l);
        }

    }
}
