using SWLOR.Game.Server;
using SWLOR.Game.Server.Event.Conversation.Quest.FinishQuest;
using static SWLOR.Game.Server.NWN.NWScript;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class fin_qst8_rule4
#pragma warning restore IDE1006 // Naming Styles
    {
        public static int Main()
        {
            return QuestComplete.Check(8, 4) ? 1 : 0;
        }
    }
}
