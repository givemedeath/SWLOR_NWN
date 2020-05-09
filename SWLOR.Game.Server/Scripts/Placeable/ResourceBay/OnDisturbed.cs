using System;
using NWN;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.ResourceBay
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
            NWPlayer player = NWScript.GetLastDisturbed();
            NWPlaceable bay = NWScript.OBJECT_SELF;
            int disturbType = NWScript.GetInventoryDisturbType();
            NWItem item = NWScript.GetInventoryDisturbItem();
            string structureID = bay.GetLocalString("PC_BASE_STRUCTURE_ID");
            Guid structureGUID = new Guid(structureID);
            var structure = DataService.PCBaseStructure.GetByID(structureGUID);
            var controlTower = BaseService.GetBaseControlTower(structure.PCBaseID);

            if (controlTower == null)
            {
                Console.WriteLine("Could not locate control tower in ResourceBay OnDisturbed. PCBaseID = " + structure.PCBaseID);
                return;
            }

            if (disturbType == NWScript.INVENTORY_DISTURB_TYPE_ADDED)
            {
                ItemService.ReturnItem(player, item);
                player.SendMessage("Items cannot be placed inside.");
                return;
            }
            else if (disturbType == NWScript.INVENTORY_DISTURB_TYPE_REMOVED)
            {
                var removeItem = DataService.PCBaseStructureItem.GetByPCBaseStructureIDAndItemGlobalIDOrDefault(controlTower.ID, item.GlobalID.ToString());
                if (removeItem == null) return;

                DataService.SubmitDataChange(removeItem, DatabaseActionType.Delete);
            }
        }
    }
}
