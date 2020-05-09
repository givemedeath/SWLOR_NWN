using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.ResourceBay
{
    public class OnOpened : IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable chest = NWScript.OBJECT_SELF;
            NWPlayer player = NWScript.GetLastOpenedBy();

            player.SendMessage("Retrieve any resources from this container. When finished, use the control tower or walk away.");

            chest.IsUseable = false;
            chest.SetLocalObject("BAY_ACCESSOR", player);
        }
    }
}
