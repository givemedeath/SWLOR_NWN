using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.TrashCan
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
            NWPlayer oPC = (NWScript.GetLastOpenedBy());
            oPC.FloatingText("Any item placed inside this trash can will be destroyed permanently.");
        }
    }
}
