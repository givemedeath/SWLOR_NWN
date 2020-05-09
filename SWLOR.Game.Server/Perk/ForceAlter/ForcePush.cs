﻿using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Perk.ForceAlter
{
    public class ForcePush: IPerkHandler
    {
        public PerkType PerkType => PerkType.ForcePush;
        public string CanCastSpell(NWCreature oPC, NWObject oTarget, int spellTier)
        {
            int size = NWScript.GetCreatureSize(oTarget);
            int maxSize = NWScript.CREATURE_SIZE_INVALID;
            switch (spellTier)
            {
                case 1:
                    maxSize = NWScript.CREATURE_SIZE_SMALL;
                    break;
                case 2:
                    maxSize = NWScript.CREATURE_SIZE_MEDIUM;
                    break;
                case 3:
                    maxSize = NWScript.CREATURE_SIZE_LARGE;
                    break;
                case 4:
                    maxSize = NWScript.CREATURE_SIZE_HUGE;
                    break;
            }

            if (size > maxSize)
                return "Your target is too large to force push.";

            return string.Empty;
        }
        
        public int FPCost(NWCreature oPC, int baseFPCost, int spellTier)
        {
            switch (spellTier)
            {
                case 1: return 4;
                case 2: return 6;
                case 3: return 8;
                case 4: return 10;
            }

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
            float duration = 0.0f;

            switch (spellTier)
            {
                case 1:
                    duration = 6f;
                    break;
                case 2:
                    duration = 12f;
                    break;
                case 3:
                    duration = 18f;
                    break;
                case 4:
                    duration = 24f;
                    break;
            }

            var result = CombatService.CalculateAbilityResistance(creature, target.Object, SkillType.ForceAlter, ForceBalanceType.Universal);


            // Resisted - Only apply slow for six seconds
            if (result.IsResisted)
            {
                NWScript.ApplyEffectToObject(DurationType.Temporary, NWScript.EffectSlow(), target, 6.0f);
            }

            // Not resisted - Apply knockdown for the specified duration
            else
            {
                // Check lucky chance.
                int luck = PerkService.GetCreaturePerkLevel(creature, PerkType.Lucky);
                if (RandomService.D100(1) <= luck)
                {
                    duration *= 2;
                    creature.SendMessage("Lucky Force Push!");
                }

                NWScript.ApplyEffectToObject(DurationType.Temporary, NWScript.EffectKnockdown(), target, duration);
            }

            if (creature.IsPlayer)
            {
                SkillService.RegisterPCToAllCombatTargetsForSkill(creature.Object, SkillType.ForceAlter, target.Object);
            }
            
            NWScript.ApplyEffectToObject(DurationType.Instant, NWScript.EffectVisualEffect(NWScript.VFX_COM_BLOOD_SPARK_SMALL), target);
        }

        public void OnPurchased(NWCreature creature, int newLevel)
        {
        }

        public void OnRemoved(NWCreature creature)
        {
        }

        public void OnItemEquipped(NWCreature creature, NWItem oItem)
        {
        }

        public void OnItemUnequipped(NWCreature creature, NWItem oItem)
        {
        }

        public void OnCustomEnmityRule(NWCreature creature, int amount)
        {
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
