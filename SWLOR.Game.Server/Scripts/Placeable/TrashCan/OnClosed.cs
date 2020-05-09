using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.TrashCan
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
            NWPlaceable self = (NWScript.OBJECT_SELF);
            foreach (NWItem item in self.InventoryItems)
            {
                item.Destroy();
            }

            self.Destroy();
        }
    }
}
