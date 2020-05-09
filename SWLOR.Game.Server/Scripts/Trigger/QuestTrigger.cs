using NWN;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Trigger
{
    public class QuestTrigger: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            QuestService.OnQuestTriggerEntered(NWScript.OBJECT_SELF);
        }
    }
}
