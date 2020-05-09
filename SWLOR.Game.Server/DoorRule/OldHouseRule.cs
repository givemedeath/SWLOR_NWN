using NWN;
using SWLOR.Game.Server.DoorRule.Contracts;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWN;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.DoorRule
{
    public class OldHouseRule: IDoorRule
    {
        public NWPlaceable Run(NWArea area, Location location, float orientationOverride = 0f, float sqrtValue = 0f)
        {
            Vector doorPosition = NWScript.GetPositionFromLocation(location);
            float fOrient = NWScript.GetFacingFromLocation(location);

            fOrient = fOrient + 126.31f;
            if (fOrient > 360.0) fOrient = fOrient - 360.0f;

            float fMod = NWScript.sqrt(13.0f) * NWScript.sin(fOrient);
            doorPosition.X = doorPosition.X + fMod;

            fMod = NWScript.sqrt(13.0f) * NWScript.cos(fOrient);
            doorPosition.Y = doorPosition.Y - fMod;
            Location doorLocation = NWScript.Location(area.Object, doorPosition, NWScript.GetFacingFromLocation(location));

            return NWScript.CreateObject(ObjectType.Placeable, "building_door", doorLocation);
        }
    }
}
