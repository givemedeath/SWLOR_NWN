using SWLOR.Game.Server;
using SWLOR.Game.Server.Event.Conversation.Quest.FinishQuest;
using static SWLOR.Game.Server.NWN.NWScript;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class fin_qst5_rule3
#pragma warning restore IDE1006 // Naming Styles
    {
        public static int Main()
        {
            return QuestComplete.Check(5, 3) ? 1 : 0;
        }
    }
}
