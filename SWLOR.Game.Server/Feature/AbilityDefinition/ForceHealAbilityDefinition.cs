﻿//using Random = SWLOR.Game.Server.Service.Random;
using System.Collections.Generic;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Core.NWScript.Enum.VisualEffect;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.AbilityService;
using static SWLOR.Game.Server.Core.NWScript.NWScript;

namespace SWLOR.Game.Server.Feature.AbilityDefinition
{
    public class ForceHealAbilityDefinition : IAbilityListDefinition
    {
        public Dictionary<Feat, AbilityDetail> BuildAbilities()
        {
            var builder = new AbilityBuilder();
            ForceHeal1(builder);
            ForceHeal2(builder);
            ForceHeal3(builder);
            ForceHeal4(builder);
            ForceHeal5(builder);

            return builder.Build();
        }

        private static void ForceHeal1(AbilityBuilder builder)
        {
            builder.Create(Feat.ForceLeap1, PerkType.ForceHeal)
                .Name("Force Heal 1")
                .HasRecastDelay(RecastGroup.ForceHeal, 60f)
                .HasActivationDelay(2.0f)
                .RequirementFP(2)
                .IsConcentrationAbility(StatusEffectType.ForceHeal1)
                .DisplaysVisualEffectWhenActivating();
        }

        private static void ForceHeal2(AbilityBuilder builder)
        {
            builder.Create(Feat.ForceLeap1, PerkType.ForceHeal)
                .Name("Force Heal II")
                .HasRecastDelay(RecastGroup.ForceHeal, 60f)
                .HasActivationDelay(2.0f)
                .RequirementFP(3)
                .IsConcentrationAbility(StatusEffectType.ForceHeal2)
                .DisplaysVisualEffectWhenActivating();
        }

        private static void ForceHeal3(AbilityBuilder builder)
        {
            builder.Create(Feat.ForceLeap1, PerkType.ForceHeal)
                .Name("Force Heal III")
                .HasRecastDelay(RecastGroup.ForceHeal, 60f)
                .HasActivationDelay(2.0f)
                .RequirementFP(4)
                .IsConcentrationAbility(StatusEffectType.ForceHeal3)
                .DisplaysVisualEffectWhenActivating();
        }

        private static void ForceHeal4(AbilityBuilder builder)
        {
            builder.Create(Feat.ForceLeap1, PerkType.ForceHeal)
                .Name("Force Heal IV")
                .HasRecastDelay(RecastGroup.ForceHeal, 60f)
                .HasActivationDelay(4.0f)
                .RequirementFP(5)
                .IsConcentrationAbility(StatusEffectType.ForceHeal4)
                .DisplaysVisualEffectWhenActivating();
        }

        private static void ForceHeal5(AbilityBuilder builder)
        {
            builder.Create(Feat.ForceLeap1, PerkType.ForceHeal)
                .Name("Force Heal V")
                .HasRecastDelay(RecastGroup.ForceHeal, 60f)
                .HasActivationDelay(4.0f)
                .RequirementFP(6)
                .IsConcentrationAbility(StatusEffectType.ForceHeal5)
                .DisplaysVisualEffectWhenActivating();
        }
    }
}