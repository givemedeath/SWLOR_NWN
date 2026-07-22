using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.StatService;
using SWLOR.NWN.API.NWScript.Enum;

namespace SWLOR.Game.Server.Tests.Service;

/// <summary>
/// Coverage for the five Shadowrun metatypes' mechanical identity.
///
/// These assert the data model rather than the live effect application, which needs a running server.
/// The point is that the roster is exactly the five canonical metatypes, that each one is
/// mechanically distinct, and that the two signature traits land on the stats shared systems already
/// read - so a troll's dermal armor flows into the soak and a dwarf's toxin resistance into poison
/// defense without any consumer special-casing race.
/// </summary>
public class MetatypeTests
{
    [Test]
    public void Roster_IsExactlyTheFiveCanonicalMetatypes()
    {
        Metatype.Metatypes.Should().BeEquivalentTo(new[]
        {
            RacialType.Human,
            RacialType.Elf,
            RacialType.Dwarf,
            RacialType.Halforc, // Ork
            RacialType.Troll,
        });
    }

    [Test]
    public void IsMetatype_IsTrueForMetatypesAndFalseForStarWarsSpecies()
    {
        Metatype.IsMetatype(RacialType.Troll).Should().BeTrue();
        Metatype.IsMetatype(RacialType.Dwarf).Should().BeTrue();

        Metatype.IsMetatype(RacialType.Wookiee).Should().BeFalse();
        Metatype.IsMetatype(RacialType.Droid).Should().BeFalse();
    }

    [Test]
    public void Human_IsTheUnmodifiedBaseline()
    {
        Metatype.GetAttributeModifiers(RacialType.Human).Should().BeEmpty(
            "humans are the flexible baseline; their identity is the reserved Edge mechanic, not stat mods");

        foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
            Metatype.GetTraitBonus(RacialType.Human, stat).Should().Be(0);
    }

    [Test]
    public void Troll_IsTheStrongDurableExtreme_WithRealTradeoffs()
    {
        var mods = Metatype.GetAttributeModifiers(RacialType.Troll);

        mods[AbilityType.Vitality].Should().BeGreaterThan(0, "a troll is the most durable metatype");
        mods[AbilityType.Might].Should().BeGreaterThan(0, "a troll is the strongest metatype");
        mods[AbilityType.Agility].Should().BeLessThan(0, "a troll is clumsy - the tradeoff that stops it being a strict upgrade");
        mods[AbilityType.Social].Should().BeLessThan(0, "a troll is off-putting");
    }

    [Test]
    public void Troll_DermalArmor_LandsOnDefense_SoItFlowsIntoTheSoak()
    {
        Metatype.GetTraitBonus(RacialType.Troll, StatType.Defense).Should().BeGreaterThan(
            0,
            "dermal armor is a flat Defense bonus, which the subtractive soak reads directly");
    }

    [Test]
    public void Dwarf_ToxinResistance_LandsOnPoisonDefense()
    {
        Metatype.GetTraitBonus(RacialType.Dwarf, StatType.PoisonDefense).Should().BeGreaterThan(
            0,
            "a dwarf's famed resistance to toxins is a PoisonDefense bonus read by shared defense systems");
    }

    [Test]
    public void EveryMetatype_IsMechanicallyDistinct()
    {
        // No two metatypes share the same attribute-modifier signature, or the roster has a reskin.
        var signatures = Metatype.Metatypes
            .Select(race => Metatype.GetAttributeModifiers(race)
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}:{kv.Value}")
                .Aggregate("", (a, b) => a + b + ";"))
            .ToList();

        signatures.Should().OnlyHaveUniqueItems("each metatype must feel different, not be a reskin");
    }

    [Test]
    public void NonMetatypeSpecies_HaveNoMetatypeModifiersOrTraits()
    {
        Metatype.GetAttributeModifiers(RacialType.Wookiee).Should().BeEmpty();
        Metatype.GetTraitBonus(RacialType.Wookiee, StatType.Defense).Should().Be(0);
    }
}
