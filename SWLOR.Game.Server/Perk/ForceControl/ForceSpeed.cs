﻿using System;
using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Perk.ForceControl
{
    public class ForceSpeed: IPerkHandler
    {
        public PerkType PerkType => PerkType.ForceSpeed;
        public string CanCastSpell(NWCreature oPC, NWObject oTarget, int spellTier)
        {
            return string.Empty;
        }
        
        public int FPCost(NWCreature oPC, int baseFPCost, int spellTier)
        {
            switch (spellTier)
            {
                case 1: return 2;
                case 2: return 4;
                case 3: return 6;
                case 4: return 8;
                case 5: return 20;
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
            Effect effect;
            float duration;
            switch (spellTier)
            {
                case 1:
                    effect = NWScript.EffectMovementSpeedIncrease(10);
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectAbilityIncrease(NWScript.ABILITY_DEXTERITY, 2));
                    duration = 60f;
                    break;
                case 2:
                    effect = NWScript.EffectMovementSpeedIncrease(20);
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectAbilityIncrease(NWScript.ABILITY_DEXTERITY, 4));
                    duration = 90f;
                    break;
                case 3:
                    effect = NWScript.EffectMovementSpeedIncrease(30);
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectAbilityIncrease(NWScript.ABILITY_DEXTERITY, 6));
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectModifyAttacks(1));
                    duration = 120f;
                    break;
                case 4:
                    effect = NWScript.EffectMovementSpeedIncrease(40);
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectAbilityIncrease(NWScript.ABILITY_DEXTERITY, 8));
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectModifyAttacks(1));
                    duration = 150f;
                    break;
                case 5:
                    effect = NWScript.EffectMovementSpeedIncrease(50);
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectAbilityIncrease(NWScript.ABILITY_DEXTERITY, 10));
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectModifyAttacks(1));
                    duration = 180f;
                    break;
                default:
                    throw new ArgumentException(nameof(perkLevel) + " invalid. Value " + perkLevel + " is unhandled.");
            }
            
            // Check lucky chance.
            int luck = PerkService.GetCreaturePerkLevel(creature, PerkType.Lucky);
            if (RandomService.D100(1) <= luck)
            {
                duration *= 2;
                creature.SendMessage("Lucky Force Speed!");
            }

            NWScript.ApplyEffectToObject(DurationType.Temporary, effect, target, duration);
            NWScript.ApplyEffectToObject(DurationType.Instant, NWScript.EffectVisualEffect(NWScript.VFX_IMP_AC_BONUS), target);

            if (creature.IsPlayer)
            {
                NWPlayer player = creature.Object;
                int skillLevel = SkillService.GetPCSkillRank(player, SkillType.ForceControl);
                int xp = skillLevel * 10 + 50;
                SkillService.GiveSkillXP(player, SkillType.ForceControl, xp);
            }
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
