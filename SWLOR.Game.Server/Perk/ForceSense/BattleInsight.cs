﻿using System;
using NWN;
using SWLOR.Game.Server.Bioware;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWN;
using static SWLOR.Game.Server.NWN.NWScript;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Perk.ForceSense
{
    public class BattleInsight: IPerkHandler
    {
        public PerkType PerkType => PerkType.BattleInsight;
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
            const float MaxDistance = 5.0f;
            int nth = 1;
            int amount;

            switch (perkLevel)
            {
                case 1:
                    amount = 5;
                    break;
                case 2:
                case 3:
                    amount = 10;
                    break;
                default: return;
            }

            // Penalize the caster
            Effect effect = NWScript.EffectACDecrease(amount);
            effect = NWScript.EffectLinkEffects(effect, NWScript.EffectAttackDecrease(amount));
            ApplyEffectToObject(DURATION_TYPE_TEMPORARY, effect, creature, 6.1f);


            NWCreature targetCreature = NWScript.GetNearestCreature(CREATURE_TYPE_IS_ALIVE, TRUE, creature, nth);
            while (targetCreature.IsValid && GetDistanceBetween(creature, targetCreature) <= MaxDistance)
            {
                // Skip the caster, if they get picked up.
                if (targetCreature == creature)
                {
                    nth++;
                    targetCreature = NWScript.GetNearestCreature(CREATURE_TYPE_IS_ALIVE, TRUE, creature, nth);
                    continue;
                }

                // Handle effects for differing spellTier values
                switch (perkLevel)
                {
                    case 1:
                        amount = 5;

                        if (NWScript.GetIsReactionTypeHostile(targetCreature, creature) == 1)
                        {
                            nth++;
                            targetCreature = NWScript.GetNearestCreature(CREATURE_TYPE_IS_ALIVE, TRUE, creature, nth);
                            continue;        
                        }                            
                        break;
                    case 2:
                        amount = 10;

                        if (NWScript.GetIsReactionTypeHostile(targetCreature, creature) == 1)
                        {
                            nth++;
                            targetCreature = NWScript.GetNearestCreature(CREATURE_TYPE_IS_ALIVE, TRUE, creature, nth);
                            continue;
                        }
                        break;
                    case 3:
                        amount = 10;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(perkLevel));
                }

                if (NWScript.GetIsReactionTypeHostile(targetCreature, creature) == 1)
                {
                    effect = NWScript.EffectACDecrease(amount);
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectAttackDecrease(amount));
                }
                else
                {
                    effect = NWScript.EffectACIncrease(amount);
                    effect = NWScript.EffectLinkEffects(effect, NWScript.EffectAttackIncrease(amount));
                }

                NWScript.ApplyEffectToObject(DurationType.Temporary, effect, targetCreature, 6.1f);
                NWScript.ApplyEffectToObject(DurationType.Instant, NWScript.EffectVisualEffect(NWScript.VFX_DUR_MAGIC_RESISTANCE), targetCreature);
                
                nth++;
                targetCreature = NWScript.GetNearestCreature(CREATURE_TYPE_IS_ALIVE, TRUE, creature, nth);
            }
            
        }
    }
}
