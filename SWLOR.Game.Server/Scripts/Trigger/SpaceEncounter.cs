using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Trigger
{
    public class SpaceEncounter: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            // Check for timeout.  Refresh every game hour.
            NWObject self = NWScript.OBJECT_SELF;
            int hour = self.GetLocalInt("HOUR");
            int day = self.GetLocalInt("DAY");
            int month = self.GetLocalInt("MONTH");
            int year = self.GetLocalInt("YEAR");

            if (NWScript.GetTimeHour() > hour || NWScript.GetCalendarDay() > day || NWScript.GetCalendarMonth() > month || NWScript.GetCalendarYear() > year)
            {
                SpaceService.CreateSpaceEncounter(NWScript.OBJECT_SELF, (NWPlayer)NWScript.GetEnteringObject());
                self.SetLocalInt("HOUR", NWScript.GetTimeHour());
                self.SetLocalInt("DAY", NWScript.GetCalendarDay());
                self.SetLocalInt("MONTH", NWScript.GetCalendarMonth());
                self.SetLocalInt("YEAR", NWScript.GetCalendarYear());
            }

        }
    }
}
