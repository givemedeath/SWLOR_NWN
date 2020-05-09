﻿using System;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Mod.Contracts;

using NWN;
using SWLOR.Game.Server.Bioware;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;


namespace SWLOR.Game.Server.Mod
{
    public class AttackBonusMod: IModHandler
    {
        public int ModTypeID => 3;
        private const int MaxValue = 20;

        public string CanApply(NWPlayer player, NWItem target, params string[] args)
        {
            if (!ItemService.WeaponBaseItemTypes.Contains(target.BaseItemType))
                return "This mod can only be applied to weapons.";

            int existingAB = GetExistingAB(target);
            if (existingAB >= MaxValue) return "You cannot improve that item's attack bonus any further.";

            return null;
        }

        public void Apply(NWPlayer player, NWItem target, params string[] args)
        {
            int additionalAB = Convert.ToInt32(args[0]);
            int existingAB = GetExistingAB(target);
            int newValue = existingAB + additionalAB;
            if (newValue > MaxValue) newValue = MaxValue;

            ItemProperty ip = NWScript.ItemPropertyAttackBonus(newValue);
            ip = NWScript.TagItemProperty(ip, "RUNE_IP");

            BiowareXP2.IPSafeAddItemProperty(target, ip, 0.0f, AddItemPropertyPolicy.ReplaceExisting, true, false);
        }

        public string Description(NWPlayer player, NWItem target, params string[] args)
        {
            int value = Convert.ToInt32(args[0]);
            return "Attack Bonus +" + value;
        }

        private int GetExistingAB(NWItem item)
        {
            int currentAB = 0;
            foreach (var ip in item.ItemProperties)
            {
                int type = NWScript.GetItemPropertyType(ip);
                if (type == NWScript.ITEM_PROPERTY_ATTACK_BONUS)
                {
                    int bonus =  NWScript.GetItemPropertyCostTableValue(ip);
                    currentAB += bonus;
                }
            }

            return currentAB;
        }
    }
}
