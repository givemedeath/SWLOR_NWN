using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.MolecularReassembler
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
            player.FloatingText("Please insert an item you would like to reassemble.");
        }
    }
}
