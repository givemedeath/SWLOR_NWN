using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable
{
    public class Sit: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWObject user = NWScript.GetLastUsedBy();
            user.AssignCommand(() =>
            {
                NWScript.ActionSit(NWScript.OBJECT_SELF);
            });
        }
    }
}
