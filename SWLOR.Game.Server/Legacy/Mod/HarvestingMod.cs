﻿using System;
using SWLOR.Game.Server.Legacy.GameObject;
using SWLOR.Game.Server.Legacy.Mod.Contracts;

namespace SWLOR.Game.Server.Legacy.Mod
{
    public class HarvestingMod : IModHandler
    {
        public int ModTypeID => 16;
        private const int MaxValue = 17;

        public string CanApply(NWPlayer player, NWItem target, params string[] args)
        {
            if (target.HarvestingBonus >= MaxValue)
                return "You cannot improve that item's harvesting bonus any further.";

            return null;
        }

        public void Apply(NWPlayer player, NWItem target, params string[] args)
        {
            var amount = Convert.ToInt32(args[0]);
            var newValue = target.HarvestingBonus + amount;
            if (newValue > MaxValue) newValue = MaxValue;
            target.HarvestingBonus = newValue;
        }

        public string Description(NWPlayer player, NWItem target, params string[] args)
        {
            var value = Convert.ToInt32(args[0]);
            return "Harvesting +" + value;
        }
    }
}