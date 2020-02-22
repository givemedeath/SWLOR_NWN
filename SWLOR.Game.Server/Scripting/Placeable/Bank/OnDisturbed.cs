using System;
using System.Linq;
using NWN;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Event.SWLOR;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Messaging;
using SWLOR.Game.Server.NWScript;
using SWLOR.Game.Server.NWScript.Enumerations;
using SWLOR.Game.Server.Scripting.Contracts;
using SWLOR.Game.Server.Service;
using _ = SWLOR.Game.Server.NWScript._;

namespace SWLOR.Game.Server.Scripting.Placeable.Bank
{
    public class OnDisturbed : IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable terminal = NWGameObject.OBJECT_SELF;
            var bankID = (Enumeration.Bank)terminal.GetLocalInt("BANK_ID");
            if (bankID == Enumeration.Bank.Invalid) return;

            NWPlayer player = _.GetLastDisturbed();
            NWItem item = _.GetInventoryDisturbItem();
            var disturbType = _.GetInventoryDisturbType();
            int itemCount = terminal.InventoryItems.Count();
            int itemLimit = terminal.GetLocalInt("BANK_LIMIT");
            if (itemLimit <= 0) itemLimit = 20;
            var dbPlayer = DataService.Player.GetByID(player.GlobalID);

            if (disturbType == InventoryDisturbType.Added)
            {
                if (_.GetHasInventory(item) == true)
                {
                    item.SetLocalBoolean("RETURNING_ITEM", true);
                    ItemService.ReturnItem(player, item);
                    player.SendMessage(ColorTokenService.Red("Containers cannot currently be stored inside banks."));
                    return;
                }

                if (itemCount > itemLimit)
                {
                    ItemService.ReturnItem(player, item);
                    player.SendMessage(ColorTokenService.Red("No more items can be placed inside."));
                }
                else
                {
                    BankItem itemEntity = new BankItem
                    {
                        ItemName = item.Name,
                        ItemTag = item.Tag,
                        ItemResref = item.Resref,
                        ItemID = item.GlobalID.ToString(),
                        ItemObject = SerializationService.Serialize(item),
                        DateStored = DateTime.UtcNow
                    };

                    dbPlayer.BankItems[bankID][item.GlobalID] = itemEntity;
                    DataService.Set(dbPlayer);
                    MessageHub.Instance.Publish(new OnStoreBankItem(player, bankID, itemEntity));
                }
            }
            else if (disturbType == InventoryDisturbType.Removed)
            {
                if (item.GetLocalBoolean("RETURNING_ITEM") == true)
                {
                    item.DeleteLocalInt("RETURNING_ITEM");
                }
                else
                {
                    // Dupe the bank item so we can send it off in an event after we remove the actual one.
                    var eventBankItem = new BankItem
                    {
                        DateStored = dbPlayer.BankItems[bankID][item.GlobalID].DateStored,
                        ItemID = dbPlayer.BankItems[bankID][item.GlobalID].ItemID,
                        ItemName = dbPlayer.BankItems[bankID][item.GlobalID].ItemName,
                        ItemObject = dbPlayer.BankItems[bankID][item.GlobalID].ItemObject,
                        ItemResref = dbPlayer.BankItems[bankID][item.GlobalID].ItemResref,
                        ItemTag = dbPlayer.BankItems[bankID][item.GlobalID].ItemTag
                    };
                    dbPlayer.BankItems[bankID].Remove(item.GlobalID);
                    DataService.Set(dbPlayer);
                    MessageHub.Instance.Publish(new OnRemoveBankItem(player, bankID, eventBankItem));
                }
            }

            player.SendMessage(ColorTokenService.White("Item Limit: " + (itemCount > itemLimit ? itemLimit : itemCount) + " / ") + ColorTokenService.Red("" + itemLimit));
        }
    }
}
