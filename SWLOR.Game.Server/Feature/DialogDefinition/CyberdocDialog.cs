using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.DialogService;
using SWLOR.Game.Server.Service.GuiService;

namespace SWLOR.Game.Server.Feature.DialogDefinition
{
    /// <summary>
    /// The street-doc conversation. Placed on a cyberclinic NPC (Conversation = "CyberdocDialog"), it
    /// opens the cyberclinic window where cyberware is installed and removed. Follows the same
    /// dialog-opens-a-window pattern as <see cref="StarportDialog"/>.
    /// </summary>
    public class CyberdocDialog : DialogBase
    {
        private const string MainPageId = "MAIN_PAGE";

        public override PlayerDialog SetUp(uint player)
        {
            var builder = new DialogBuilder()
                .AddPage(MainPageId, MainPageInit);

            return builder.Build();
        }

        private void MainPageInit(DialogPage page)
        {
            page.Header =
                ColorToken.Green("Cyberclinic") + "\n\n" +
                "You want chrome, I got chrome. Costs nuyen, costs a piece of your soul - Essence, they call it. " +
                "The more iron in you, the less magic you'll ever touch. Your call, chummer.";

            page.AddResponse("Browse cyberware", () =>
            {
                var player = GetPC();
                EndConversation();
                Gui.TogglePlayerWindow(player, GuiWindowType.Cyberware);
            });

            page.AddResponse("Maybe later", EndConversation);
        }
    }
}
