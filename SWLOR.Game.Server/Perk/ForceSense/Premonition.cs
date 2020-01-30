using System;
using System.Collections.Generic;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWScript.Enumerations;
using SWLOR.Game.Server.NWScript;
using Skill = SWLOR.Game.Server.Enumeration.Skill;
using SWLOR.Game.Server.Service;

namespace SWLOR.Game.Server.Perk.ForceSense
{
    public class Premonition : IPerk
    {
        public PerkType PerkType => PerkType.Premonition;
        public string Name => "Premonition";
        public bool IsActive => true;
        public string Description => "The caster sees a short way into the future, allowing them to prepare to ward against their foes.";
        public PerkCategoryType Category => PerkCategoryType.ForceSense;
        public PerkCooldownGroup CooldownGroup => PerkCooldownGroup.Premonition;
        public PerkExecutionType ExecutionType => PerkExecutionType.ConcentrationAbility;
        public bool IsTargetSelfOnly => true;
        public int Enmity => 0;
        public EnmityAdjustmentRuleType EnmityAdjustmentType => EnmityAdjustmentRuleType.None;
        public ForceBalanceType ForceBalanceType => ForceBalanceType.Universal;
        public Animation CastAnimation => Animation.Invalid;

        public string CanCastSpell(NWCreature oPC, NWObject oTarget, int spellTier)
        {
            return string.Empty;
        }

        public int FPCost(NWCreature oPC, int baseFPCost, int spellTier)
        {
            return baseFPCost;
        }

        public float CastingTime(NWCreature oPC, int spellTier)
        {
            return 0f;
        }

        public float CooldownTime(NWCreature oPC, float baseCooldownTime, int spellTier)
        {
            return baseCooldownTime;
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

        public Dictionary<int, PerkLevel> PerkLevels => new Dictionary<int, PerkLevel>
        {
            {
                1, new PerkLevel(4, "The next time the caster would die in the next 30 minutes, they are instead healed to 25% of their max HP.",
                new Dictionary<Skill, int>
                {
                    { Skill.ForceSense, 0},
                })
            },
            {
                2, new PerkLevel(7, "The next time the caster would die in the next 30 minutes, they are instead healed to 50% of their max HP.",
                new Dictionary<Skill, int>
                {
                    { Skill.ForceSense, 20},
                })
            },
            {
                3, new PerkLevel(10, "For 12s after casting, the caster is immune to all damage, and the next time the caster would die in the next 30 minutes, they are instead healed to 25% of their max HP.", SpecializationType.Sentinel,
                new Dictionary<Skill, int>
                {
                    { Skill.ForceSense, 50},
                })
            },
        };


        public Dictionary<int, List<PerkFeat>> PerkFeats { get; } = new Dictionary<int, List<PerkFeat>>
        {
            {
                1, new List<PerkFeat>
                {
                    new PerkFeat {Feat = Feat.Premonition1, BaseFPCost = 10, ConcentrationFPCost = 5, ConcentrationTickInterval = 6}
                }
            },
            {
                2, new List<PerkFeat>
                {
                    new PerkFeat {Feat = Feat.Premonition2, BaseFPCost = 10, ConcentrationFPCost = 6, ConcentrationTickInterval = 6}
                }
            },
            {
                3, new List<PerkFeat>
                {
                    new PerkFeat {Feat = Feat.Premonition3, BaseFPCost = 10, ConcentrationFPCost = 7, ConcentrationTickInterval = 6}
                }
            },
        };

        public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
            Effect effect;
            float duration = 6.1f;
            int concealment;
            int hitpoints;

            switch (perkLevel)
            {

                case 1:
                    hitpoints = 10;
                    effect = _.EffectTemporaryHitpoints(hitpoints);
                    break;
                case 2:
                    hitpoints = 20;
                    effect = _.EffectTemporaryHitpoints(hitpoints);
                    break;
                case 3:
                    concealment = 5;
                    hitpoints = 30;
                    effect = _.EffectConcealment(concealment);
                    effect = _.EffectLinkEffects(effect, _.EffectTemporaryHitpoints(hitpoints));
                    break;
                default:
                    throw new ArgumentException(nameof(perkLevel) + " invalid. Value " + perkLevel + " is unhandled.");


            }

            _.ApplyEffectToObject(DurationType.Temporary, effect, creature, duration);
            _.ApplyEffectToObject(DurationType.Temporary, _.EffectVisualEffect(Vfx.Vfx_Dur_Aura_Purple), creature, duration);

            if (_.GetIsInCombat(creature))
            {
                if (creature.IsPlayer)
                {
                    SkillService.GiveSkillXP(creature.Object, Skill.ForceSense, (perkLevel * 50));
                }
            }
        }
    }
}
