using FluentAssertions;
using NUnit.Framework;
using SWLOR.Game.Server.Entity;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.StatService;

namespace SWLOR.Game.Server.Tests.Service;

/// <summary>
/// Coverage for the cyberware mechanics that must hold before a live playtest: the Essence budget,
/// the passive stat aggregation, and the chrome-versus-magic curve.
///
/// The seed catalogue is loaded once here rather than through the module event so the rules are
/// tested against the real definitions.
/// </summary>
public class CyberwareTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Discover the seed cyberware the same way the module load does.
        Cyberware.CacheData();
    }

    private static Player NewPlayer()
    {
        return new Player("test");
    }

    [Test]
    public void SeedCatalogue_Loaded()
    {
        Cyberware.GetAll().Should().NotBeEmpty();
        Cyberware.Exists("wired_reflexes").Should().BeTrue();
    }

    [Test]
    public void FreshCharacter_HasFullEssenceAndNoChrome()
    {
        var dbPlayer = NewPlayer();

        dbPlayer.InstalledCyberware.Should().BeEmpty();
        dbPlayer.EssenceSpent.Should().Be(0f);
        Cyberware.GetEssenceAvailable(dbPlayer).Should().Be(Cyberware.MaxEssence);
    }

    [Test]
    public void Installing_SpendsEssence_AndGrantsTheStat()
    {
        var dbPlayer = NewPlayer();

        Cyberware.AddInstalled(dbPlayer, "dermal_plating");

        dbPlayer.EssenceSpent.Should().Be(1.0f);
        Cyberware.GetEssenceAvailable(dbPlayer).Should().Be(5.0f);
        Cyberware.GetStatBonus(dbPlayer, StatType.Defense).Should().Be(4);
    }

    [Test]
    public void Removing_RefundsEssence_AndRevokesTheStat()
    {
        var dbPlayer = NewPlayer();
        Cyberware.AddInstalled(dbPlayer, "dermal_plating");

        Cyberware.RemoveInstalled(dbPlayer, "dermal_plating");

        dbPlayer.EssenceSpent.Should().Be(0f);
        dbPlayer.InstalledCyberware.Should().BeEmpty();
        Cyberware.GetStatBonus(dbPlayer, StatType.Defense).Should().Be(0);
    }

    [Test]
    public void StatBonuses_FromDifferentPieces_Aggregate()
    {
        var dbPlayer = NewPlayer();
        Cyberware.AddInstalled(dbPlayer, "wired_reflexes");   // Evasion +6, Attack +4
        Cyberware.AddInstalled(dbPlayer, "reaction_enhancers"); // Evasion +5

        Cyberware.GetStatBonus(dbPlayer, StatType.Evasion).Should().Be(11);
        Cyberware.GetStatBonus(dbPlayer, StatType.Attack).Should().Be(4);
    }

    [Test]
    public void EssenceBudget_AllowsUpToTheLimit_AndRejectsBeyondIt()
    {
        var dbPlayer = NewPlayer();

        // Exactly enough room for a 0.5 piece.
        dbPlayer.EssenceSpent = 5.5f;
        Cyberware.GetInstallBlockReason(dbPlayer, "cybereyes", 0)
            .Should().BeEmpty("0.5 Essence fits in the remaining 0.5");

        // Not enough room.
        dbPlayer.EssenceSpent = 5.6f;
        Cyberware.GetInstallBlockReason(dbPlayer, "cybereyes", 0)
            .Should().Contain("Essence", "0.5 Essence does not fit in the remaining 0.4");
    }

    [Test]
    public void FullChrome_SpendsExactlyTheWholeBudget()
    {
        var dbPlayer = NewPlayer();
        foreach (var id in Cyberware.GetAll().Keys)
            Cyberware.AddInstalled(dbPlayer, id);

        dbPlayer.EssenceSpent.Should().Be(Cyberware.MaxEssence,
            "the seed catalogue is tuned so installing everything spends exactly the 6.0 budget");
        Cyberware.GetEssenceAvailable(dbPlayer).Should().Be(0f);
    }

    [Test]
    public void GetInstallBlockReason_RejectsDuplicateAndUnknown()
    {
        var dbPlayer = NewPlayer();
        Cyberware.AddInstalled(dbPlayer, "cybereyes");

        Cyberware.GetInstallBlockReason(dbPlayer, "cybereyes", 0)
            .Should().Contain("already installed");
        Cyberware.GetInstallBlockReason(dbPlayer, "no_such_ware", 0)
            .Should().Contain("does not exist");
    }

    // ---- Magic loss -----------------------------------------------------------------------------

    [Test]
    public void MagicLoss_IsZeroAtFullEssence()
    {
        Stat.ApplyEssenceMagicLoss(60, 0f).Should().Be(60);
    }

    [Test]
    public void MagicLoss_IsProportionalToEssenceSpent()
    {
        // Half the Essence gone -> half the Magic.
        Stat.ApplyEssenceMagicLoss(60, 3f).Should().Be(30);
        // All chrome -> essentially no Magic.
        Stat.ApplyEssenceMagicLoss(60, 6f).Should().Be(0);
    }

    [Test]
    public void MagicLoss_IsMonotonic_AndNeverNegative()
    {
        var previous = int.MaxValue;
        for (var tenths = 0; tenths <= 60; tenths++)
        {
            var loss = Stat.ApplyEssenceMagicLoss(60, tenths / 10f);
            loss.Should().BeLessThanOrEqualTo(previous, "more Essence spent must never raise Magic");
            loss.Should().BeGreaterThanOrEqualTo(0);
            previous = loss;
        }
    }

    [Test]
    public void MagicLoss_BarelyTouchesANonCaster()
    {
        // A non-caster's FP is small (base 10 + a little Willpower); losing a fraction of it is
        // negligible, which is why "reduces Magic for all" needs no caster flag.
        var casterHit = 60 - Stat.ApplyEssenceMagicLoss(60, 6f);      // 60
        var nonCasterHit = 12 - Stat.ApplyEssenceMagicLoss(12, 6f);   // 12
        nonCasterHit.Should().BeLessThan(casterHit);
    }
}
