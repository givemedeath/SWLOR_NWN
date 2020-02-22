using System;
using System.Linq;
using NWN;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;

using SWLOR.Game.Server.ValueObject.Dialog;
using static SWLOR.Game.Server.NWScript._;
using _ = SWLOR.Game.Server.NWScript._;

namespace SWLOR.Game.Server.Conversation
{
    public class ItemImpoundRetrieval: ConversationBase
    {
        public override PlayerDialog SetUp(NWPlayer player)
        {
            PlayerDialog dialog = new PlayerDialog("MainPage");
            DialogPage mainPage = new DialogPage("Welcome to the planetary impound. Any items which were seized by our government may be retrieved here... for a price, of course!\n\nEach item may be retrieved for 50 credits.\n\nHow may I help you?");
            
            dialog.AddPage("MainPage", mainPage);
            return dialog;
        }

        public override void Initialize()
        {
            LoadMainPage();
        }

        private void LoadMainPage()
        {
            var player = GetPC();
            var dbPlayer = DataService.Player.GetByID(player.GlobalID);
            var items = dbPlayer.ImpoundedItems;

            ClearPageResponses("MainPage");
            foreach (var item in items)
            {
                AddResponseToPage("MainPage", item.Value.ItemName, true, item.Key);
            }
        }

        public override void DoAction(NWPlayer player, string pageName, int responseID)
        {
            if (player.Gold < 50)
            {
                player.FloatingText("You don't have enough credits to retrieve that item.");
                return;
            }

            var response = GetResponseByID("MainPage", responseID);
            var itemId = (Guid)response.CustomData;
            var dbPlayer = DataService.Player.GetByID(player.GlobalID);
            var item = dbPlayer.ImpoundedItems[itemId];

            dbPlayer.ImpoundedItems.Remove(itemId);
            DataService.Set(dbPlayer);
            SerializationService.DeserializeItem(item.ItemObject, player);
            _.TakeGoldFromCreature(50, player, true);

            LoadMainPage();
        }

        public override void Back(NWPlayer player, string beforeMovePage, string afterMovePage)
        {
        }

        public override void EndDialog()
        {
        }
    }
}
