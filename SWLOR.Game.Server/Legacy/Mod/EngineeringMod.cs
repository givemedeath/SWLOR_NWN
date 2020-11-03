﻿using System;
using SWLOR.Game.Server.Legacy.GameObject;
using SWLOR.Game.Server.Legacy.Mod.Contracts;

namespace SWLOR.Game.Server.Legacy.Mod
{
    public class EngineeringMod : IModHandler
    {
        public int ModTypeID => 17;
        private const int MaxValue = 17;

        public string CanApply(NWPlayer player, NWItem target, params string[] args)
        {
            if (target.CraftBonusEngineering >= MaxValue)
                return "You cannot improve that item's engineering bonus any further.";

            return null;
        }

        public void Apply(NWPlayer player, NWItem target, params string[] args)
        {
            var amount = Convert.ToInt32(args[0]);
            var newValue = target.CraftBonusEngineering + amount;
            if (newValue > MaxValue) newValue = MaxValue;
            target.CraftBonusEngineering = newValue;
        }

        public string Description(NWPlayer player, NWItem target, params string[] args)
        {
            var value = Convert.ToInt32(args[0]);
            return "Engineering +" + value;
        }
    }
}