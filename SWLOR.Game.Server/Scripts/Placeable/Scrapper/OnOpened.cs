using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.Scrapper
{
    public class OnOpened: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlayer player = NWScript.GetLastOpenedBy();
            NWPlaceable container = NWScript.OBJECT_SELF;
            player.FloatingText("Components placed inside this container will have all bonuses stripped and their level will be reduced to zero.");
            container.IsLocked = true;
            container.IsUseable = false;
        }
    }
}
