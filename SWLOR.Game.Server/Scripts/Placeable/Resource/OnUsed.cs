using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.Resource
{
    public class OnUsed: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWCreature user = (NWScript.GetLastUsedBy());
            user.SendMessage("Use a scanner to analyze this object's resources. Use a harvester to retrieve resources from it.");
        }
    }
}
