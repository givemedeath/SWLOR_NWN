using FluentAssertions;
using NUnit.Framework;
using SWLOR.Game.Server.Feature.StatusEffectDefinition;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.StatService;

namespace SWLOR.Game.Server.Tests.Service;

/// <summary>
/// Coverage for the glitch rules - the third of D1's behavioural fixes.
///
/// The two pieces are pure by design (mirroring CalculateHitRate/CalculateCriticalRate) so the
/// competence curve and the minor-versus-critical classification are tested without a running server;
/// the rolling and effect application on top of them is a thin wrapper.
/// </summary>
public class GlitchTests
{
    [Test]
    public void GlitchRate_IsHighestForAnUnskilledAttacker()
    {
        Combat.CalculateGlitchRate(0).Should().Be(Combat.BaseGlitchRate);
    }

    [Test]
    public void GlitchRate_FallsAsAccuracyRises_ButNeverBelowTheFloor()
    {
        var previous = int.MaxValue;
        for (var accuracy = 0; accuracy <= 300; accuracy += 5)
        {
            var rate = Combat.CalculateGlitchRate(accuracy);

            rate.Should().BeLessThanOrEqualTo(previous, "more accuracy must never raise the glitch chance");
            rate.Should().BeInRange(Combat.MinimumGlitchRate, Combat.BaseGlitchRate);
            previous = rate;
        }
    }

    [Test]
    public void GlitchRate_AVeteranGlitchesLessThanAGreenRunner()
    {
        Combat.CalculateGlitchRate(90).Should().BeLessThan(Combat.CalculateGlitchRate(30));
    }

    [Test]
    public void ResolveGlitch_IsNone_WhenTheRollBeatsTheRate()
    {
        // Roll above the rate: no glitch, whether the attack hit or missed.
        Combat.ResolveGlitch(true, 50, 5).Should().Be(Combat.GlitchOutcome.None);
        Combat.ResolveGlitch(false, 50, 5).Should().Be(Combat.GlitchOutcome.None);
    }

    [Test]
    public void ResolveGlitch_OnAHit_IsMinor()
    {
        Combat.ResolveGlitch(true, 3, 5).Should().Be(Combat.GlitchOutcome.Minor);
    }

    [Test]
    public void ResolveGlitch_OnAMiss_IsCritical()
    {
        Combat.ResolveGlitch(false, 3, 5).Should().Be(Combat.GlitchOutcome.Critical);
    }

    [Test]
    public void ResolveGlitch_FiresExactlyOnTheRate_Boundary()
    {
        // roll == rate glitches; roll == rate + 1 does not.
        Combat.ResolveGlitch(true, 5, 5).Should().NotBe(Combat.GlitchOutcome.None);
        Combat.ResolveGlitch(true, 6, 5).Should().Be(Combat.GlitchOutcome.None);
    }

    [Test]
    public void MinorGlitch_DebuffsAccuracyOnly()
    {
        var effect = new GlitchStatusEffect();

        effect.StatGroup.Stats[StatType.AccuracyPercentAdjustment].Should().BeLessThan(0);
        effect.StatGroup.Stats[StatType.EvasionPercentAdjustment].Should().Be(0,
            "a minor glitch only fouls the attacker's aim, not their footwork");
    }

    [Test]
    public void CriticalGlitch_DebuffsAccuracyAndEvasion_MoreThanTheMinorGlitch()
    {
        var minor = new GlitchStatusEffect();
        var critical = new CriticalGlitchStatusEffect();

        critical.StatGroup.Stats[StatType.AccuracyPercentAdjustment]
            .Should().BeLessThan(minor.StatGroup.Stats[StatType.AccuracyPercentAdjustment],
                "a critical glitch hurts more than a minor one");
        critical.StatGroup.Stats[StatType.EvasionPercentAdjustment].Should().BeLessThan(0);
    }
}
