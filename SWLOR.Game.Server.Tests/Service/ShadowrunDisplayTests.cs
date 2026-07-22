using FluentAssertions;
using NUnit.Framework;
using SWLOR.Game.Server.Service;

namespace SWLOR.Game.Server.Tests.Service;

/// <summary>
/// Coverage for the Shadowrun display translation.
///
/// These assert across ranges rather than spot values on purpose. The failure players find first is
/// not a single wrong number - it is two different ratings that render as the same pool while
/// producing visibly different hit rates, or a rating that renders lower than a weaker one. Both are
/// range properties and invisible to hand-picked cases.
/// </summary>
public class ShadowrunDisplayTests
{
    /// <summary>Comfortably past any player rating, to cover overtuned bosses.</summary>
    private const int OvertunedRating = 600;

    private static void AssertNonDecreasing(Func<int, int> map, int from, int to, string label)
    {
        var previous = map(from);

        for (var input = from + 1; input <= to; input++)
        {
            var current = map(input);

            current.Should().BeGreaterThanOrEqualTo(
                previous,
                "{0} must never decrease as its input rises (input {1})",
                label,
                input);

            previous = current;
        }
    }

    [Test]
    public void AttackPool_IsNonDecreasing_AcrossFullRange()
    {
        AssertNonDecreasing(ShadowrunDisplay.GetAttackPool, -50, OvertunedRating, "Attack pool");
    }

    [Test]
    public void DefensePool_IsNonDecreasing_AcrossFullRange()
    {
        AssertNonDecreasing(ShadowrunDisplay.GetDefensePool, -50, OvertunedRating, "Defense pool");
    }

    [Test]
    public void Pools_AreFlooredAtZero_ForRatingsBelowTheBaseOffset()
    {
        for (var rating = -50; rating <= ShadowrunDisplay.PoolBaseOffset; rating++)
        {
            ShadowrunDisplay.GetAttackPool(rating).Should().BeGreaterThanOrEqualTo(0);
        }
    }

    /// <summary>
    /// The overtuning guarantee. Boss and elite stats come from an uncapped stat skin and sit far
    /// outside player range; a ceiling would render them identically to a strong player and hide the
    /// threat. This is the assertion that fails if someone later adds a "sensible" clamp.
    /// </summary>
    [Test]
    public void Pools_AreUnbounded_SoOvertunedEnemiesStayDistinguishable()
    {
        // A rating at the top of realistic player range: rank 50, a level-cap attribute, some gear.
        const int strongPlayerRating = 8 + (2 * 50) + 40 + 10;

        var strongPlayer = ShadowrunDisplay.GetAttackPool(strongPlayerRating);
        var boss = ShadowrunDisplay.GetAttackPool(strongPlayerRating * 3);
        var greaterBoss = ShadowrunDisplay.GetAttackPool(strongPlayerRating * 6);

        boss.Should().BeGreaterThan(strongPlayer);
        greaterBoss.Should().BeGreaterThan(boss);
    }

    [Test]
    public void AttackPool_LandsInTheCalibratedBand_AcrossPlayerRange()
    {
        // Weakest plausible character: no ranks, a low attribute, no gear.
        var weakest = ShadowrunDisplay.GetAttackPool(8 + 0 + 8);

        // Strongest plausible character: rank cap, a level-cap attribute, good gear.
        var strongest = ShadowrunDisplay.GetAttackPool(8 + (2 * 50) + 40 + 10);

        weakest.Should().BeInRange(0, 3);
        strongest.Should().BeInRange(15, 22);
    }

    [Test]
    public void SkillRating_IsNonDecreasing_AndCoversPlayerRangeInOneToSeven()
    {
        AssertNonDecreasing(ShadowrunDisplay.GetSkillRating, 0, 200, "Skill rating");

        ShadowrunDisplay.GetSkillRating(0).Should().Be(0);
        ShadowrunDisplay.GetSkillRating(1).Should().Be(1);
        ShadowrunDisplay.GetSkillRating(50).Should().Be(7);

        // NPC ranks may exceed the player cap and must not be clamped to 7.
        ShadowrunDisplay.GetSkillRating(120).Should().BeGreaterThan(7);
    }

    [Test]
    public void AttributeRating_IsNonDecreasing_AndFlooredAtZero()
    {
        AssertNonDecreasing(ShadowrunDisplay.GetAttributeRating, -20, 200, "Attribute rating");

        ShadowrunDisplay.GetAttributeRating(0).Should().Be(0);
        ShadowrunDisplay.GetAttributeRating(-5).Should().Be(0);
    }

    [Test]
    public void ConditionBoxes_AreNonDecreasing_AndStayWithinTheMonitor()
    {
        const int maximum = 250;

        AssertNonDecreasing(
            current => ShadowrunDisplay.GetPhysicalConditionBoxes(current, maximum),
            0,
            maximum,
            "Condition boxes");

        for (var current = 0; current <= maximum; current++)
        {
            ShadowrunDisplay
                .GetPhysicalConditionBoxes(current, maximum)
                .Should()
                .BeInRange(0, ShadowrunDisplay.ConditionMonitorBoxes);
        }
    }

    [Test]
    public void ConditionBoxes_AreFullAtFullHealth_AndEmptyAtZero()
    {
        ShadowrunDisplay.GetPhysicalConditionBoxes(250, 250)
            .Should().Be(ShadowrunDisplay.ConditionMonitorBoxes);

        ShadowrunDisplay.GetPhysicalConditionBoxes(0, 250).Should().Be(0);
    }

    /// <summary>
    /// The monitor communicates how hurt something is, not how much health it has, so an overtuned
    /// boss at half health must read the same as a player at half health.
    /// </summary>
    [Test]
    public void ConditionBoxes_AreProportional_SoOvertunedHealthPoolsReadTheSame()
    {
        var player = ShadowrunDisplay.GetPhysicalConditionBoxes(125, 250);
        var boss = ShadowrunDisplay.GetPhysicalConditionBoxes(25_000, 50_000);

        boss.Should().Be(player);
    }

    [Test]
    public void ConditionBoxes_HandleDegenerateInput()
    {
        ShadowrunDisplay.GetPhysicalConditionBoxes(10, 0).Should().Be(0);
        ShadowrunDisplay.GetPhysicalConditionBoxes(-5, 250).Should().Be(0);

        // Overhealed past maximum must not overflow the monitor.
        ShadowrunDisplay.GetPhysicalConditionBoxes(400, 250)
            .Should().Be(ShadowrunDisplay.ConditionMonitorBoxes);
    }

    [Test]
    public void StunBoxes_TrackStaminaOnTheSameScaleAsPhysical()
    {
        ShadowrunDisplay.GetStunConditionBoxes(60, 120)
            .Should().Be(ShadowrunDisplay.GetPhysicalConditionBoxes(60, 120));
    }

    [Test]
    public void DamageValueAndMagicPool_PassThrough_AndFloorAtZero()
    {
        ShadowrunDisplay.GetDamageValue(14).Should().Be(14);
        ShadowrunDisplay.GetDamageValue(-3).Should().Be(0);

        ShadowrunDisplay.GetMagicPool(4).Should().Be(4);
        ShadowrunDisplay.GetMagicPool(-1).Should().Be(0);
    }

    /// <summary>
    /// The Magic pool must stay 1:1 with Force Points. Compressing it onto an Edge-sized scale would
    /// hide most individual casts - ability costs run 2-9 against a pool in the tens or hundreds - and
    /// a resource players read mid-fight has to resolve every spend. This is the assertion that fails
    /// if someone later "tidies" the pool onto a 1-7 range.
    /// </summary>
    [Test]
    public void MagicPool_ResolvesEverySpend_AndIsNeverCompressed()
    {
        const int maxPool = 150;

        for (var pool = 0; pool <= maxPool; pool++)
        {
            ShadowrunDisplay.GetMagicPool(pool).Should().Be(pool);
        }

        // The cheapest ability cost in the game must still move the displayed value.
        const int cheapestAbilityCost = 2;
        ShadowrunDisplay.GetMagicPool(maxPool)
            .Should().NotBe(ShadowrunDisplay.GetMagicPool(maxPool - cheapestAbilityCost));
    }

    [Test]
    public void FormatOpposedTest_RendersPoolsRatherThanAPercentage()
    {
        const int accuracy = 8 + (2 * 40) + 20;
        const int evasion = 8 + (2 * 30) + 15;

        var formatted = ShadowrunDisplay.FormatOpposedTest(accuracy, evasion);

        formatted.Should().Be(
            $"(Pool {ShadowrunDisplay.GetAttackPool(accuracy)} vs {ShadowrunDisplay.GetDefensePool(evasion)})");
        formatted.Should().NotContain("%");
    }
}
