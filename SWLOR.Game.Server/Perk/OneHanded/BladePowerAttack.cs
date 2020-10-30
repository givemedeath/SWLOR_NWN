﻿using SWLOR.Game.Server.Core.NWNX;
using SWLOR.Game.Server.Core.NWScript;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Service;

namespace SWLOR.Game.Server.Perk.OneHanded
{
    public class BladePowerAttack : IPerkHandler
    {
        public PerkType PerkType => PerkType.BladePowerAttack;

        public string CanCastSpell(NWCreature oPC, NWObject oTarget, int spellTier)
        {
            return string.Empty;
        }
        
        public int FPCost(NWCreature oPC, int baseFPCost, int spellTier)
        {
            return baseFPCost;
        }

        public float CastingTime(NWCreature oPC, float baseCastingTime, int spellTier)
        {
            return baseCastingTime;
        }

        public float CooldownTime(NWCreature oPC, float baseCooldownTime, int spellTier)
        {
            return baseCooldownTime;
        }

        public int? CooldownCategoryID(NWCreature creature, int? baseCooldownCategoryID, int spellTier)
        {
            return baseCooldownCategoryID;
        }

        public void OnImpact(NWCreature creature, NWObject target, int perkLevel, int spellTier)
        {
        }

        public void OnPurchased(NWCreature creature, int newLevel)
        {
            ApplyFeatChanges(creature, null);
        }

        public void OnRemoved(NWCreature creature)
        {
            Creature.RemoveFeat(creature, Feat.PowerAttack);
            Creature.RemoveFeat(creature, Feat.ImprovedPowerAttack);
        }

        public void OnItemEquipped(NWCreature creature, NWItem oItem)
        {
            if (oItem.CustomItemType != CustomItemType.Vibroblade) return;
            ApplyFeatChanges(creature, null);
        }

        public void OnItemUnequipped(NWCreature creature, NWItem oItem)
        {
            if (oItem.CustomItemType != CustomItemType.Vibroblade) return;
            if (oItem == creature.LeftHand) return;

            ApplyFeatChanges(creature, oItem);
        }

        public void OnCustomEnmityRule(NWCreature creature, int amount)
        {
        }

        private void ApplyFeatChanges(NWCreature creature, NWItem oItem)
        {
            var equipped = oItem ?? creature.RightHand;
            
            if (Equals(equipped, oItem) || equipped.CustomItemType != CustomItemType.Vibroblade)
            {
                Creature.RemoveFeat(creature, Feat.PowerAttack);
                Creature.RemoveFeat(creature, Feat.ImprovedPowerAttack);
                if (NWScript.GetActionMode(creature, ActionMode.PowerAttack) == true)
                {
                    NWScript.SetActionMode(creature, ActionMode.PowerAttack, false);
                }
                if (NWScript.GetActionMode(creature, ActionMode.ImprovedPowerAttack) == true)
                {
                    NWScript.SetActionMode(creature, ActionMode.ImprovedPowerAttack, false);
                }
                return;
            }

            var perkLevel = PerkService.GetCreaturePerkLevel(creature, PerkType.BladePowerAttack);
            Creature.AddFeat(creature, Feat.PowerAttack);

            if (perkLevel >= 2)
            {
                Creature.AddFeat(creature, Feat.ImprovedPowerAttack);
            }
        }

        public bool IsHostile()
        {
            return false;
        }

        public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
            
        }
    }
}
