using System.Linq;
using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.ScavengePoint
{
    public class OnClosed: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable point = (NWScript.OBJECT_SELF);
            NWPlayer player = NWScript.GetLastClosedBy();

            if (!player.IsPlayer) return;

            bool isFullyHarvested = point.GetLocalInt("SCAVENGE_POINT_FULLY_HARVESTED") == 1;

            if (!point.InventoryItems.Any() && isFullyHarvested)
            {
                point.Destroy();
            }
        }
    }
}
