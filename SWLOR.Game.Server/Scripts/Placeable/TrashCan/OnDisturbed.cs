using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.TrashCan
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
            NWItem oItem = (NWScript.GetInventoryDisturbItem());
            int type = NWScript.GetInventoryDisturbType();

            if (type == NWScript.INVENTORY_DISTURB_TYPE_ADDED)
            {
                oItem.Destroy();
            }
        }
    }
}
