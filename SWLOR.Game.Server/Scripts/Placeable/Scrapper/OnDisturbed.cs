﻿using System;
using System.Linq;
using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWNX;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.Scrapper
{
    public class OnDisturbed: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            int type = NWScript.GetInventoryDisturbType();
            if (type != NWScript.INVENTORY_DISTURB_TYPE_ADDED) return;
            NWPlaceable device = NWScript.OBJECT_SELF;
            NWPlayer player = NWScript.GetLastDisturbed();
            NWItem item = NWScript.GetInventoryDisturbItem();
            var componentIP = item.ItemProperties.FirstOrDefault(x => NWScript.GetItemPropertyType(x) == (int)ItemPropertyType.ComponentType);

            // Not a component. Return the item.
            if (componentIP == null)
            {
                ItemService.ReturnItem(player, item);
                player.FloatingText("You cannot scrap this item.");
                return;
            }

            // Item is a component but it was crafted. Cannot scrap crafted items.
            if (!string.IsNullOrWhiteSpace(item.GetLocalString("CRAFTER_PLAYER_ID")))
            {
                ItemService.ReturnItem(player, item);
                player.FloatingText("You cannot scrap crafted items.");
                return;
            }

            // Remove the item properties
            foreach (var ip in item.ItemProperties)
            {
                var ipType = NWScript.GetItemPropertyType(ip);
                if (ipType != (int)ItemPropertyType.ComponentType)
                {
                    NWScript.RemoveItemProperty(item, ip);
                }
            }
            
            // Remove local variables (except the global ID)
            int varCount = NWNXObject.GetLocalVariableCount(item);
            for (int index = varCount-1; index >= 0; index--)
            {
                var localVar = NWNXObject.GetLocalVariable(item, index);

                if (localVar.Key != "GLOBAL_ID")
                {
                    switch (localVar.Type)
                    {
                        case LocalVariableType.Int:
                            item.DeleteLocalInt(localVar.Key);
                            break;
                        case LocalVariableType.Float:
                            item.DeleteLocalFloat(localVar.Key);
                            break;
                        case LocalVariableType.String:
                            item.DeleteLocalString(localVar.Key);
                            break;
                        case LocalVariableType.Object:
                            item.DeleteLocalObject(localVar.Key);
                            break;
                        case LocalVariableType.Location:
                            item.DeleteLocalLocation(localVar.Key);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            if (!item.Name.Contains("(SCRAPPED)"))
            {
                item.Name = item.Name + " (SCRAPPED)";
            }

            device.AssignCommand(() =>
            {
                NWScript.ActionGiveItem(item, player);
            });

            return;
        }
    }
}
