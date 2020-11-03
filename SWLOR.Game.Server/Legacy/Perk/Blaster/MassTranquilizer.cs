﻿using System.Linq;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Core.NWScript.Enum.Creature;
using SWLOR.Game.Server.Core.NWScript.Enum.VisualEffect;
using SWLOR.Game.Server.Legacy.Enumeration;
using SWLOR.Game.Server.Legacy.GameObject;
using SWLOR.Game.Server.Legacy.Service;
using static SWLOR.Game.Server.Core.NWScript.NWScript;
using PerkType = SWLOR.Game.Server.Legacy.Enumeration.PerkType;

namespace SWLOR.Game.Server.Legacy.Perk.Blaster
{
    public class MassTranquilizer : IPerkHandler
    {
        public PerkType PerkType => PerkType.MassTranquilizer;

        public string CanCastSpell(NWCreature oPC, NWObject oTarget, int spellTier)
        {
            if (oPC.RightHand.CustomItemType != CustomItemType.BlasterRifle)
                return "Must be equipped with a blaster rifle to use that ability.";

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
            var massLevel = PerkService.GetCreaturePerkLevel(creature, PerkType.MassTranquilizer);
            var tranqLevel = PerkService.GetCreaturePerkLevel(creature, PerkType.Tranquilizer);
            var luck = PerkService.GetCreaturePerkLevel(creature, PerkType.Lucky);
            float duration;
            float range = 5 * massLevel;
            
            switch (tranqLevel)
            {
                case 0:
                    duration = 6;
                    break;
                case 1:
                    duration = 12;
                    break;
                case 2:
                    duration = 24;
                    break;
                case 3:
                    duration = 36;
                    break;
                case 4:
                    duration = 48;
                    break;
                case 5:
                    duration = 60;
                    break;
                case 6:
                    duration = 72;
                    break;
                case 7:
                    duration = 84;
                    break;
                case 8:
                    duration = 96;
                    break;
                case 9:
                    duration = 108;
                    break;
                case 10:
                    duration = 120;
                    break;
                default: return;
            }

            if (SWLOR.Game.Server.Service.Random.D100(1) <= luck)
            {
                duration *= 2;
                creature.SendMessage("Lucky shot!");
            }


            // Check if Mind Shield is on target.
            var concentrationEffect = AbilityService.GetActiveConcentrationEffect(target.Object);
            if (concentrationEffect.Type == PerkType.MindShield ||
                GetIsImmune(target, ImmunityType.MindSpells) == true)
            {
                creature.SendMessage("Your target is immune to tranquilization effects.");
            }
            else
            {
                // Apply to the target.
                if (!RemoveExistingEffect(target, duration))
                {
                    target.SetLocalInt("TRANQUILIZER_EFFECT_FIRST_RUN", 1);

                    var effect = EffectDazed();
                    effect = EffectLinkEffects(effect, EffectVisualEffect(VisualEffect.Vfx_Dur_Iounstone_Blue));
                    effect = TagEffect(effect, "TRANQUILIZER_EFFECT");

                    ApplyEffectToObject(DurationType.Temporary, effect, target, duration);
                }
            }



            // Iterate over all nearby hostiles. Apply the effect to them if they meet the criteria.
            var current = 1;
            NWCreature nearest = GetNearestCreature(CreatureType.IsAlive, 1, target, current);
            while (nearest.IsValid)
            {
                var distance = GetDistanceBetween(nearest, target);
                // Check distance. Exit loop if we're too far.
                if (distance > range) break;

                concentrationEffect = AbilityService.GetActiveConcentrationEffect(nearest);

                // If this creature isn't hostile to the attacking player or if this creature is already tranquilized, move to the next one.
                if (GetIsReactionTypeHostile(nearest, creature) == false ||
                    nearest.Object == target.Object ||
                    RemoveExistingEffect(nearest, duration) ||
                    concentrationEffect.Type == PerkType.MindShield)
                {
                    current++;
                    nearest = GetNearestCreature(CreatureType.IsAlive, 1, target, current);
                    continue;
                }

                target.SetLocalInt("TRANQUILIZER_EFFECT_FIRST_RUN", 1);
                var effect = EffectDazed();
                effect = EffectLinkEffects(effect, EffectVisualEffect(VisualEffect.Vfx_Dur_Iounstone_Blue));
                effect = TagEffect(effect, "TRANQUILIZER_EFFECT");
                ApplyEffectToObject(DurationType.Temporary, effect, nearest, duration);

                current++;
                nearest = GetNearestCreature(CreatureType.IsAlive, 1, target, current);
            }

        }

        private bool RemoveExistingEffect(NWObject target, float duration)
        {
            var effect = target.Effects.FirstOrDefault(x => GetEffectTag(x) == "TRANQUILIZER_EFFECT");
            if (effect == null) return false;

            if (GetEffectDurationRemaining(effect) >= duration) return true;
            RemoveEffect(target, effect);
            return false;
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