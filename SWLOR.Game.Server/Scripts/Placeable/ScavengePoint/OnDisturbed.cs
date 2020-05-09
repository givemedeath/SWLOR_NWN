using System.Linq;
using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.ScavengePoint
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
            NWPlayer oPC = (NWScript.GetLastDisturbed());
            if (!oPC.IsPlayer) return;

            NWItem oItem = (NWScript.GetInventoryDisturbItem());
            NWPlaceable point = (NWScript.OBJECT_SELF);
            int disturbType = NWScript.GetInventoryDisturbType();

            if (disturbType == NWScript.INVENTORY_DISTURB_TYPE_ADDED)
            {
                ItemService.ReturnItem(oPC, oItem);
            }
            else
            {
                if (!point.InventoryItems.Any() && point.GetLocalInt("SCAVENGE_POINT_FULLY_HARVESTED") == 1)
                {
                    string seed = point.GetLocalString("SCAVENGE_POINT_SEED");
                    if (!string.IsNullOrWhiteSpace(seed))
                    {
                        NWScript.CreateObject(NWScript.OBJECT_TYPE_ITEM, seed, point.Location);

                        int perkLevel = PerkService.GetCreaturePerkLevel(oPC, PerkType.SeedPicker);
                        if (RandomService.Random(100) + 1 <= perkLevel * 10)
                        {
                            NWScript.CreateObject(NWScript.OBJECT_TYPE_ITEM, seed, point.Location);
                        }
                    }

                    point.Destroy();
                }
            }
        }
    }
}
