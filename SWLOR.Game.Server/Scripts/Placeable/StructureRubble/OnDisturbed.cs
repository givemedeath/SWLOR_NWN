using System.Linq;
using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.StructureRubble
{
    public class OnDisturbed: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            int disturbType = NWScript.GetInventoryDisturbType();
            NWItem item = (NWScript.GetInventoryDisturbItem());
            NWCreature creature = (NWScript.GetLastDisturbed());
            NWPlaceable container = (NWScript.OBJECT_SELF);

            if (disturbType == NWScript.INVENTORY_DISTURB_TYPE_ADDED)
            {
                ItemService.ReturnItem(creature, item);
            }
            else if (disturbType == NWScript.INVENTORY_DISTURB_TYPE_REMOVED)
            {
                if (!container.InventoryItems.Any())
                {
                    container.Destroy();
                }
            }
        }
    }
}
