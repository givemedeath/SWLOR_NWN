using SWLOR.Game.Server;
using SWLOR.Game.Server.Event.Conversation.Quest.OnQuestState;
using static NWN._;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    internal class on_qst10_state_2
#pragma warning restore IDE1006 // Naming Styles
    {
        public static int Main()
        {
            return QuestCheckState.Check(10, 2) ? 1 : 0;
        }
    }
}
