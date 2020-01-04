﻿using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;

namespace SWLOR.Game.Server.Event.SWLOR
{
    public class OnReassembleComplete
    {
        public NWPlayer Player { get; set; }
        public string SerializedSalvageItem { get; set; }
        public ComponentType SalvageComponentTypeID { get; set; }

        public OnReassembleComplete(NWPlayer player, string serializedSalvageItem, ComponentType salvageComponentTypeID)
        {
            Player = player;
            SerializedSalvageItem = serializedSalvageItem;
            SalvageComponentTypeID = salvageComponentTypeID;
        }
    }
}
