using System;
using System.Collections.Generic;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.NWScript;
using SWLOR.Game.Server.NWScript.Enumerations;
using Skill = SWLOR.Game.Server.Enumeration.Skill;

namespace SWLOR.Game.Server.Perk.ForceSense
{
    public class AnimalBond : IPerk
    {
        public PerkType PerkType => PerkType.AnimalBond;
        public string Name => "Animal Bond";
        public bool IsActive => false;
        public string Description => "The caster convinces a creature to travel and fight with them.";
        public PerkCategoryType Category => PerkCategoryType.ForceSense;
        public PerkCooldownGroup CooldownGroup => PerkCooldownGroup.AnimalBond;
        public PerkExecutionType ExecutionType => PerkExecutionType.ForceAbility;
        public bool IsTargetSelfOnly => false;
        public int Enmity => 0;
        public EnmityAdjustmentRuleType EnmityAdjustmentType => EnmityAdjustmentRuleType.None;
        public ForceBalanceType ForceBalanceType => ForceBalanceType.Universal;
        public Animation CastAnimation => Animation.Invalid;

        public string CanCastSpell(NWCreature oPC, NWObject oTarget, int perkLevel)
        {
            NWCreature targetCreature = oTarget.Object;

            switch (perkLevel)
            {
                case 1:
                    if (!oTarget.IsCreature)
                        return "This ability can only be used on living creatures.";
                    if (oTarget.GetLocalInt("FORCE_BOND_IMMUNITY") == 1)
                    {
                        oPC.SendMessage("Creature Immune To Animal Bonding");
                    }
                    if (targetCreature.RacialType != RacialType.Animal)
                        return "This ability can only be used on animals or vermin.";
                    {
                        if (targetCreature.RacialType != RacialType.Vermin)
                            return "This ability can only be used on animals or vermin.";
                    }
                    break;
                case 2:
                    if (!oTarget.IsCreature)
                        return "This ability can only be used on living creatures.";
                    if (oTarget.GetLocalInt("FORCE_BOND_IMMUNITY") == 1)
                    {
                        oPC.SendMessage("Creature Immune To Animal Bonding");
                    }
                    else if (targetCreature.RacialType != RacialType.Animal)
                        return "This ability can only be used on animals or vermin.";
                    else if (targetCreature.RacialType != RacialType.Vermin)
                        return "This ability can only be used on animals or vermin.";
                    break;
                case 3:
                    if (!oTarget.IsCreature)
                        return "This ability can only be used on living creatures.";
                    if (oTarget.GetLocalInt("FORCE_BOND_IMMUNITY") == 1)
                    {
                        oPC.SendMessage("Creature Immune To Animal Bonding");
                    }
                    else if (targetCreature.RacialType != RacialType.Animal)
                        return "This ability can only be used on animals or vermin.";
                    else if (targetCreature.RacialType != RacialType.Vermin)
                        return "This ability can only be used on animals or vermin.";
                    break;
                case 4:
                    if (!oTarget.IsCreature)
                        return "This ability can only be used on living creatures.";
                    if (oTarget.GetLocalInt("FORCE_BOND_IMMUNITY") == 1)
                    {
                        oPC.SendMessage("Creature Immune To Animal Bonding");
                    }
                    else if (targetCreature.RacialType != RacialType.Animal)
                        return "This ability can only be used on animals or vermin.";
                    else if (targetCreature.RacialType != RacialType.Vermin)
                        return "This ability can only be used on animals or vermin.";
                    break;
                case 5:
                    if (!oTarget.IsCreature)
                        return "This ability can only be used on living creatures.";
                    if (oTarget.GetLocalInt("FORCE_BOND_IMMUNITY") == 1)
                    {
                        oPC.SendMessage("Creature Immune To Animal Bonding");
                    }
                    else if (targetCreature.RacialType != RacialType.Animal)
                        return "This ability can only be used on animals or vermin.";
                    else if (targetCreature.RacialType != RacialType.Vermin)
                        return "This ability can only be used on animals or vermin.";
                    break;
                case 6:
                    if (!oTarget.IsCreature)
                        return "This ability can only be used on living creatures.";
                    if (oTarget.GetLocalInt("FORCE_BOND_IMMUNITY") == 1)
                    {
                        oPC.SendMessage("Creature Immune To Animal Bonding");
                    }
                    else if (targetCreature.RacialType != RacialType.Animal)
                        return "This ability can only be used on animals or vermin.";
                    else if (targetCreature.RacialType != RacialType.Vermin)
                        return "This ability can only be used on animals or vermin.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(perkLevel));
            }

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
            ApplyEffect(creature, target, perkLevel);
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
                1, new PerkLevel(2, "The caster befriends an animal or beast with up to Challenge Rating 4.",
                new Dictionary<Skill, int>
                {
                    { Skill.ForceSense, 10},
                })
            },
            {
                2, new PerkLevel(2, "The caster befriends an animal or beast with up to Challenge Rating 8.",
                new Dictionary<Skill, int>
                {
                    { Skill.ForceSense, 25},
                })
            },
            {
                3, new PerkLevel(3, "The caster befriends an animal or beast with up to Challenge Rating 12.",
                new Dictionary<Skill, int>
                {
                    { Skill.ForceSense, 40},
                })
            },
            {
                4, new PerkLevel(3, "The caster befriends an animal or beast with up to Challenge Rating 16.", SpecializationType.Sentinel,
                new Dictionary<Skill, int>
                {
                    { Skill.ForceSense, 55},
                })
            },
            {
                5, new PerkLevel(4, "The caster befriends an animal or beast with up to Challenge Rating 20.", SpecializationType.Sentinel,
                new Dictionary<Skill, int>
                {
                    { Skill.ForceSense, 70},
                })
            },
            {
                6, new PerkLevel(5, "The caster befriends an animal or beast with any Challenge Rating.", SpecializationType.Sentinel,
                new Dictionary<Skill, int>
                {
                    { Skill.ForceSense, 85},
                })
            },
        };


        public Dictionary<int, List<PerkFeat>> PerkFeats { get; } = new Dictionary<int, List<PerkFeat>>
        {
            {
                1, new List<PerkFeat>
                {
                    new PerkFeat {Feat = Feat.AnimalBond1, BaseFPCost = 20, ConcentrationFPCost = 0, ConcentrationTickInterval = 0}
                }
            },
            {
                2, new List<PerkFeat>
                {
                    new PerkFeat {Feat = Feat.AnimalBond2, BaseFPCost = 25, ConcentrationFPCost = 0, ConcentrationTickInterval = 0}
                }
            },
            {
                3, new List<PerkFeat>
                {
                    new PerkFeat {Feat = Feat.AnimalBond3, BaseFPCost = 30, ConcentrationFPCost = 0, ConcentrationTickInterval = 0}
                }
            },
            {
                4, new List<PerkFeat>
                {
                    new PerkFeat {Feat = Feat.AnimalBond4, BaseFPCost = 35, ConcentrationFPCost = 0, ConcentrationTickInterval = 0}
                }
            },
            {
                5, new List<PerkFeat>
                {
                    new PerkFeat {Feat = Feat.AnimalBond5, BaseFPCost = 40, ConcentrationFPCost = 0, ConcentrationTickInterval = 0}
                }
            },
            {
                6, new List<PerkFeat>
                {
                    new PerkFeat {Feat = Feat.AnimalBond6, BaseFPCost = 45, ConcentrationFPCost = 0, ConcentrationTickInterval = 0}
                }
            },
        };

        public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
        }

        private void ApplyEffect(NWCreature creature, NWObject target, int perkLevel)
        {
            Effect effect = _.EffectDominated();
            int ABCR;
            float duration = 300f;
            int CRXP = ((int)_.GetChallengeRating(target));

            switch (perkLevel)
            {
                case 1:
                    ABCR = 4;
                    if (ABCR >= _.GetChallengeRating(target))
                    {
                        creature.AssignCommand(() =>
                        {
                            _.ApplyEffectToObject(DurationType.Temporary, effect, target, duration);
                            _.ApplyEffectToObject(DurationType.Temporary, _.EffectVisualEffect(Vfx.Vfx_Dur_Mind_Affecting_Dominated), target, duration);
                        });
                        // Give Sense XP, if player.
                        if (creature.IsPlayer)
                        {
                            SkillService.GiveSkillXP(creature.Object, Skill.ForceSense, (CRXP * 100));
                        }
                    }
                    else
                    {
                        creature.SendMessage("Bonding failed.");
                    }
                    break;
                case 2:
                    ABCR = 8;
                    if (ABCR >= _.GetChallengeRating(target))
                    {
                        creature.AssignCommand(() =>
                        {
                            _.ApplyEffectToObject(DurationType.Temporary, effect, target, duration);
                            _.ApplyEffectToObject(DurationType.Temporary, _.EffectVisualEffect(Vfx.Vfx_Dur_Mind_Affecting_Dominated), target, duration);
                        });
                        // Give Sense XP, if player.
                        if (creature.IsPlayer)
                        {
                            SkillService.GiveSkillXP(creature.Object, Skill.ForceSense, (CRXP * 100));
                        }
                    }
                    else
                    {
                        creature.SendMessage("Bonding failed.");
                    }
                    break;
                case 3:
                    ABCR = 12;
                    if (ABCR >= _.GetChallengeRating(target))
                    {
                        creature.AssignCommand(() =>
                        {
                            _.ApplyEffectToObject(DurationType.Temporary, effect, target, duration);
                            _.ApplyEffectToObject(DurationType.Temporary, _.EffectVisualEffect(Vfx.Vfx_Dur_Mind_Affecting_Dominated), target, duration);
                        });
                        // Give Sense XP, if player.
                        if (creature.IsPlayer)
                        {
                            SkillService.GiveSkillXP(creature.Object, Skill.ForceSense, (CRXP * 100));
                        }
                    }
                    else
                    {
                        creature.SendMessage("Bonding failed.");
                    }
                    break;
                case 4:
                    ABCR = 16;
                    if (ABCR >= _.GetChallengeRating(target))
                    {
                        creature.AssignCommand(() =>
                        {
                            _.ApplyEffectToObject(DurationType.Temporary, effect, target, duration);
                            _.ApplyEffectToObject(DurationType.Temporary, _.EffectVisualEffect(Vfx.Vfx_Dur_Mind_Affecting_Dominated), target, duration);
                        });
                        // Give Sense XP, if player.
                        if (creature.IsPlayer)
                        {
                            SkillService.GiveSkillXP(creature.Object, Skill.ForceSense, (CRXP * 100));
                        }
                    }
                    else
                    {
                        creature.SendMessage("Bonding failed.");
                    }
                    break;
                case 5:
                    ABCR = 20;
                    if (ABCR >= _.GetChallengeRating(target))
                    {
                        creature.AssignCommand(() =>
                        {
                            _.ApplyEffectToObject(DurationType.Temporary, effect, target, duration);
                            _.ApplyEffectToObject(DurationType.Temporary, _.EffectVisualEffect(Vfx.Vfx_Dur_Mind_Affecting_Dominated), target, duration);
                        });
                        // Give Sense XP, if player.
                        if (creature.IsPlayer)
                        {
                            SkillService.GiveSkillXP(creature.Object, Skill.ForceSense, (CRXP * 100));
                        }
                    }
                    else
                    {
                        creature.SendMessage("Bonding failed.");
                    }
                    break;
                case 6:
                    ABCR = 0;
                    if (ABCR > _.GetChallengeRating(target))
                    {
                        creature.AssignCommand(() =>
                        {
                            _.ApplyEffectToObject(DurationType.Temporary, effect, target, duration);
                            _.ApplyEffectToObject(DurationType.Temporary, _.EffectVisualEffect(Vfx.Vfx_Dur_Mind_Affecting_Dominated), target, duration);
                        });
                        // Give Sense XP, if player.
                        if (creature.IsPlayer)
                        {
                            SkillService.GiveSkillXP(creature.Object, Skill.ForceSense, (CRXP * 100));
                        }
                    }
                    break;
            }
        }
    }
}
