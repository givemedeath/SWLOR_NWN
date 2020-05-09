using SWLOR.Game.Server.Event.Module;
using SWLOR.Game.Server.Messaging;
using SWLOR.Game.Server.NWNX;
using static SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Service
{
    public class FeedbackService
    {
        public static void SubscribeEvents()
        {
            MessageHub.Instance.Subscribe<OnModuleLoad>(msg => OnModuleLoad());
        }

        private static void OnModuleLoad()
        {
            NWNXFeedback.SetFeedbackMessageHidden(FeedbackMessageTypes.UseItemCantUse, TRUE);
            NWNXFeedback.SetFeedbackMessageHidden(FeedbackMessageTypes.CombatRunningOutOfAmmo, TRUE);
        }
    }
}
