﻿using System;
using System.Linq;
using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.ValueObject;

using static SWLOR.Game.Server.NWN.NWScript;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Perk.ForceControl
{
    public class MindShield: IPerkHandler
    {
        public PerkType PerkType => PerkType.MindShield;
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

        public void OnConcentrationTick(NWCreature creature, NWObject target, int spellTier, int tick)
        {
            ApplyEffect(creature, target, spellTier);
        }

        private void ApplyEffect(NWCreature creature, NWObject target, int spellTier)
        {
            Effect effectMindShield;

            // Handle effects for differing spellTier values
            switch (spellTier)
            {
                case 1:
                    effectMindShield =NWScript.EffectImmunity(IMMUNITY_TYPE_DAZED);

                    creature.AssignCommand(() =>
                    {
                        NWScript.ApplyEffectToObject(DurationType.Temporary, effectMindShield, target, 6.1f);
                    });
                    break;
                case 2:
                    effectMindShield = NWScript.EffectImmunity(IMMUNITY_TYPE_DAZED);
                    effectMindShield = NWScript.EffectLinkEffects(effectMindShield, NWScript.EffectImmunity(IMMUNITY_TYPE_CONFUSED));
                    effectMindShield = NWScript.EffectLinkEffects(effectMindShield, NWScript.EffectImmunity(IMMUNITY_TYPE_DOMINATE)); // Force Pursuade is DOMINATION effect

                    creature.AssignCommand(() =>
                    {
                        NWScript.ApplyEffectToObject(DurationType.Temporary, effectMindShield, target, 6.1f);
                    });
                    break;
                case 3:
                    effectMindShield = NWScript.EffectImmunity(IMMUNITY_TYPE_DAZED);
                    effectMindShield = NWScript.EffectLinkEffects(effectMindShield, NWScript.EffectImmunity(IMMUNITY_TYPE_CONFUSED));
                    effectMindShield = NWScript.EffectLinkEffects(effectMindShield, NWScript.EffectImmunity(IMMUNITY_TYPE_DOMINATE)); // Force Pursuade is DOMINATION effect

                    if (target.GetLocalInt("FORCE_DRAIN_IMMUNITY") == 1)
                
                    creature.SetLocalInt("FORCE_DRAIN_IMMUNITY",0);                   
                    creature.DelayAssignCommand(() =>
                    {
                        creature.DeleteLocalInt("FORCE_DRAIN_IMMUNITY");
                    },6.1f);

                    creature.AssignCommand(() =>
                    {
                        NWScript.ApplyEffectToObject(DurationType.Temporary, effectMindShield, target, 6.1f);
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spellTier));
            }

            // Play VFX
            NWScript.ApplyEffectToObject(DurationType.Instant, NWScript.EffectVisualEffect(NWScript.VFX_DUR_MIND_AFFECTING_POSITIVE), target);

        }
    }
}
