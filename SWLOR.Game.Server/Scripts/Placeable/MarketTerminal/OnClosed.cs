using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.MarketTerminal
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
            // Clean up temporary data.
            NWPlayer player = NWScript.GetLastClosedBy();
            NWPlaceable device = NWScript.OBJECT_SELF;
            var model = MarketService.GetPlayerMarketData(player);
            device.DestroyAllInventoryItems();
            device.IsLocked = false;
            
            NWScript.SetEventScript(device.Object, NWScript.EVENT_SCRIPT_PLACEABLE_ON_USED, "script_1");
            NWScript.SetEventScript(device.Object, NWScript.EVENT_SCRIPT_PLACEABLE_ON_OPEN, string.Empty);
            NWScript.SetEventScript(device.Object, NWScript.EVENT_SCRIPT_PLACEABLE_ON_CLOSED, string.Empty);
            NWScript.SetEventScript(device.Object, NWScript.EVENT_SCRIPT_PLACEABLE_ON_INVENTORYDISTURBED, string.Empty);
            
            // Only wipe the data if we're not returning from an item preview for item picking.
            if(!model.IsReturningFromItemPreview &&
               !model.IsReturningFromItemPicking)
                MarketService.ClearPlayerMarketData(player);
        }
    }
}
