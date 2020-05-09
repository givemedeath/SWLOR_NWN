using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.CraftingDevice
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
            // Should only fire when a player walks away from the device.
            // Clean up temporary data and return all items placed inside.
            NWPlayer player = (NWScript.GetLastClosedBy());
            NWPlaceable device = (NWScript.OBJECT_SELF);
            var model = CraftService.GetPlayerCraftingData(player);
            device.DestroyAllInventoryItems();
            device.IsLocked = false;
            model.IsAccessingStorage = false;

            foreach (var item in model.MainComponents)
            {
                NWScript.CopyItem(item.Object, player.Object, NWScript.TRUE);
                item.Destroy();
            }
            foreach (var item in model.SecondaryComponents)
            {
                NWScript.CopyItem(item.Object, player.Object, NWScript.TRUE);
                item.Destroy();
            }
            foreach (var item in model.TertiaryComponents)
            {
                NWScript.CopyItem(item.Object, player.Object, NWScript.TRUE);
                item.Destroy();
            }
            foreach (var item in model.EnhancementComponents)
            {
                NWScript.CopyItem(item.Object, player.Object, NWScript.TRUE);
                item.Destroy();
            }

            NWScript.SetEventScript(device.Object, NWScript.EVENT_SCRIPT_PLACEABLE_ON_USED, "script_1");
            NWScript.SetEventScript(device.Object, NWScript.EVENT_SCRIPT_PLACEABLE_ON_OPEN, string.Empty);
            NWScript.SetEventScript(device.Object, NWScript.EVENT_SCRIPT_PLACEABLE_ON_CLOSED, string.Empty);
            NWScript.SetEventScript(device.Object, NWScript.EVENT_SCRIPT_PLACEABLE_ON_INVENTORYDISTURBED, string.Empty);
            player.Data.Remove("CRAFTING_MODEL");
        }
    }
}
