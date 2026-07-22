using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SWLOR.Game.Server.Service;

namespace SWLOR.Game.Server.Tests.Service
{
    /// <summary>
    /// Measures how combat <em>feels</em>, not whether it computes.
    ///
    /// The Shadowrun conversion is a presentation layer over SWLOR's math, so the risk is not that
    /// the arithmetic is wrong - <see cref="CombatDamageTests"/> covers that - but that the resulting
    /// texture reads as an MMO grind wearing Shadowrun vocabulary. Texture is measurable: it is
    /// mostly time-to-kill, hit frequency, and whether armor produces a visible threshold.
    ///
    /// This fixture exists because of how the parity-soak bug shipped. The suite asserted that a
    /// weak attack bounces off heavy armor, which was true and passing, while an evenly matched
    /// low-level fight dealt zero damage in both directions. Every assertion was green and combat
    /// was non-functional. Spot assertions on individual formulas cannot catch that class of
    /// failure; only simulating a whole fight can.
    ///
    /// Profiles are taken from the shipped module rather than invented, so the numbers describe the
    /// game as it exists. Run with <c>NUnit.DisplayOutput</c> or read the test output to see the
    /// full report.
    /// </summary>
    [TestFixture]
    public class CombatFeelHarnessTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // CalculateSoakDamageRange writes to the Attack log group, which throws unless the
            // logger is registered. Same setup as CombatDamageTests.
            Environment.SetEnvironmentVariable(
                "SWLOR_APP_LOG_DIRECTORY",
                Path.Combine(TestContext.CurrentContext.WorkDirectory, "logs") + Path.DirectorySeparatorChar);
            Log.Register();
        }

        /// <summary>
        /// A combatant expressed in the values the combat math actually consumes.
        ///
        /// Accuracy and Evasion follow <c>8 + 2*level + stat + bonus</c> from
        /// <see cref="Stat.GetAccuracy"/> and <see cref="Stat.GetEvasion"/>. Attack, Defense, DMG,
        /// and HP are read from NPC stat skins as item properties.
        /// </summary>
        private sealed record Combatant(
            string Name,
            int Level,
            int HP,
            int Accuracy,
            int Evasion,
            int Attack,
            int Defense,
            int DMG,
            int Stat,
            int DefStat);

        /// <summary>
        /// Tiers sampled from <c>Module/utc</c>, using the median creature in each level band.
        ///
        /// The "Street" row is the Ashwing Echo (Mynock) verbatim - the weakest enemy in the module
        /// and the one a new character meets first. It is included because it is the exact fight
        /// that the flat parity soak broke, so it must stay in the report permanently.
        /// </summary>
        private static IEnumerable<(Combatant Attacker, Combatant Defender, string Label)> Matchups()
        {
            var street = new Combatant("Ashwing Echo", 2, 114, 18, 18, 3, 5, 7, 6, 6);
            var newbie = new Combatant("New runner", 2, 100, 20, 20, 5, 5, 5, 8, 6);

            var gangerL = new Combatant("Ganger", 10, 215, 38, 38, 9, 10, 22, 12, 12);
            var runnerL = new Combatant("Runner", 10, 220, 40, 38, 12, 12, 24, 14, 12);

            var vetE = new Combatant("Veteran enemy", 20, 345, 58, 58, 9, 15, 30, 18, 18);
            var vetP = new Combatant("Veteran runner", 20, 350, 60, 58, 16, 18, 34, 20, 18);

            var primeE = new Combatant("Prime enemy", 45, 2160, 118, 118, 21, 25, 74, 30, 30);
            var primeP = new Combatant("Prime runner", 45, 2200, 120, 118, 30, 30, 80, 32, 30);

            var boss = new Combatant("Dark Lord (boss)", 100, 12899, 238, 238, 38, 44, 178, 45, 45);

            yield return (street, newbie, "Street: Mynock -> new character");
            yield return (newbie, street, "Street: new character -> Mynock");
            yield return (runnerL, gangerL, "Low: runner -> ganger");
            yield return (vetP, vetE, "Mid: veteran -> veteran enemy");
            yield return (primeP, primeE, "High: prime runner -> prime enemy");
            yield return (primeP, boss, "Boss: prime runner -> Dark Lord");
            yield return (boss, primeP, "Boss: Dark Lord -> prime runner");
        }

        private sealed record FightResult(
            string Label,
            int HitRatePercent,
            int AttackPool,
            int DefensePool,
            double MeanDamagePerLandedHit,
            double MeanDamagePerExchange,
            double ExchangesToKill,
            double PercentOfHealthPerHit);

        /// <summary>
        /// The three knobs that govern combat texture, held as data so the harness can measure a
        /// candidate without the engine having to adopt it first.
        ///
        /// <see cref="PercentPerPool"/> is expressed per <em>displayed</em> pool step rather than per
        /// rating point, because that is the unit players actually see. Shipped combat resolves at
        /// <c>75 + (ACC-EVA)/2</c>, and since a pool step is
        /// <see cref="ShadowrunDisplay.PoolDivisor"/> rating points, one displayed pool is worth only
        /// four percentage points against a 75% floor. That is why every even matchup in the shipped
        /// numbers reads ~76% no matter what the pools say.
        /// </summary>
        private sealed record Tuning(
            string Name,
            int BaseHitRate,
            double PercentPerPool,
            int MinHitRate,
            int MaxHitRate,
            double HealthScale)
        {
            /// <summary>
            /// The live engine configuration, read from the real constants so the report describes
            /// the game as it is rather than a copy that can drift out of sync with it.
            /// </summary>
            public static Tuning Shipped => new(
                "shipped",
                Combat.BaseHitRate,
                Combat.HitRatePercentPerPool,
                Combat.MinimumHitRate,
                Combat.MaximumHitRate,
                1d / Combat.NPCHealthCurveDivisor);

            /// <summary>
            /// The configuration before the feel retune: a 75% base with a shallow slope and the
            /// unscaled health curve. Kept so the report can show what changed and why.
            /// </summary>
            public static Tuning Original => new(
                "original",
                75,
                ShadowrunDisplay.PoolDivisor / 2d,
                20,
                95,
                1.0d);

            public int HitRate(int accuracy, int evasion)
            {
                var perRating = PercentPerPool / ShadowrunDisplay.PoolDivisor;
                var rate = BaseHitRate + (int)Math.Floor((accuracy - evasion) * perRating);

                return Math.Clamp(rate, MinHitRate, MaxHitRate);
            }
        }

        /// <summary>
        /// Simulates repeated exchanges and reports the aggregate texture.
        ///
        /// Deliberately models only the auto-attack loop: no abilities, no criticals, no positioning.
        /// That understates real burst damage, so exchange counts here are an <em>upper</em> bound on
        /// how long a fight feels. If the auto-attack loop already reads as Shadowrun-paced, the real
        /// fight will too; if it reads as a grind, abilities have to carry the entire difference.
        /// </summary>
        private static FightResult Simulate(
            Combatant attacker,
            Combatant defender,
            string label,
            Tuning tuning)
        {
            const int Trials = 20000;

            // Fixed seed: the report has to be reproducible run to run, or it cannot be used to
            // compare a tuning change against the previous numbers.
            var rng = new System.Random(20260722);

            var hitRate = tuning.HitRate(attacker.Accuracy, defender.Evasion);
            var effectiveHP = Math.Max(1, (int)Math.Round(defender.HP * tuning.HealthScale));
            var (min, max) = Combat.CalculateSoakDamageRange(
                attacker.Attack,
                attacker.DMG,
                attacker.Stat,
                defender.Defense,
                defender.DefStat,
                0);

            long totalDamage = 0;
            long landed = 0;

            for (var i = 0; i < Trials; i++)
            {
                if (rng.Next(1, 101) > hitRate)
                    continue;

                landed++;
                totalDamage += max <= min ? min : rng.Next(min, max + 1);
            }

            var meanPerLanded = landed == 0 ? 0d : totalDamage / (double)landed;
            var meanPerExchange = totalDamage / (double)Trials;
            var exchangesToKill = meanPerExchange <= 0d
                ? double.PositiveInfinity
                : effectiveHP / meanPerExchange;

            return new FightResult(
                label,
                hitRate,
                ShadowrunDisplay.GetAttackPool(attacker.Accuracy),
                ShadowrunDisplay.GetDefensePool(defender.Evasion),
                meanPerLanded,
                meanPerExchange,
                exchangesToKill,
                defender.HP <= 0 ? 0d : meanPerLanded / defender.HP * 100d);
        }

        private static List<FightResult> RunAll(Tuning tuning)
        {
            return Matchups().Select(m => Simulate(m.Attacker, m.Defender, m.Label, tuning)).ToList();
        }

        private static List<FightResult> RunAll() => RunAll(Tuning.Shipped);

        /// <summary>
        /// Matchups where both sides are within a pool step of each other. These carry the feel
        /// verdict: a boss is <em>supposed</em> to be lopsided, so including overtuned fights in a
        /// pacing target would tune the wrong thing.
        /// </summary>
        private static bool IsEvenlyMatched(FightResult r) => Math.Abs(r.AttackPool - r.DefensePool) <= 1;

        /// <summary>Shadowrun firefights resolve in roughly this many landed attacks.</summary>
        private const double TargetExchangesMin = 3d;
        private const double TargetExchangesMax = 12d;

        private static string Render(string heading, Tuning tuning, List<FightResult> results)
        {
            var sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine(heading);
            sb.AppendLine(
                $"  base {tuning.BaseHitRate}%, {tuning.PercentPerPool:F0}% per displayed pool, " +
                $"clamp [{tuning.MinHitRate},{tuning.MaxHitRate}], health x{tuning.HealthScale:F2}");
            sb.AppendLine(new string('-', 100));
            sb.AppendLine($"{"matchup",-38}{"pools",7}{"hit%",6}{"dmg/hit",10}{"dmg/xchg",10}{"exchanges",11}{"%HP/hit",8}");
            sb.AppendLine(new string('-', 100));

            foreach (var r in results)
            {
                var exchanges = double.IsInfinity(r.ExchangesToKill)
                    ? "never"
                    : r.ExchangesToKill.ToString("F1");
                var flag = IsEvenlyMatched(r) &&
                           (r.ExchangesToKill < TargetExchangesMin || r.ExchangesToKill > TargetExchangesMax)
                    ? " <-"
                    : string.Empty;

                sb.AppendLine(
                    $"{r.Label,-38}{$"{r.AttackPool}v{r.DefensePool}",7}{r.HitRatePercent,6}" +
                    $"{r.MeanDamagePerLandedHit,10:F1}{r.MeanDamagePerExchange,10:F1}" +
                    $"{exchanges,11}{r.PercentOfHealthPerHit,8:F2}{flag}");
            }

            sb.AppendLine(new string('-', 100));

            return sb.ToString();
        }

        [Test]
        public void CombatFeel_Report()
        {
            TestContext.Out.WriteLine(Render(
                "BEFORE - original tuning, auto-attack loop only",
                Tuning.Original,
                RunAll(Tuning.Original)));

            var shipped = RunAll(Tuning.Shipped);

            TestContext.Out.WriteLine(Render(
                "AFTER - live engine constants",
                Tuning.Shipped,
                shipped));
            TestContext.Out.WriteLine(
                $"  target for evenly matched fights: {TargetExchangesMin:F0}-{TargetExchangesMax:F0} exchanges" +
                " ('<-' marks a miss)");

            shipped.Should().NotBeEmpty();
        }

        /// <summary>
        /// The failure this harness was built to catch, kept as a permanent guard: if every evenly
        /// matched fight resolves at the same hit rate, the dice pools shown to players are
        /// decorative - the numbers on screen move and the outcome does not.
        ///
        /// The original tuning failed this outright, at a spread of two points across pools running
        /// from 1 to 14.
        /// </summary>
        [Test]
        public void EvenlyMatchedFights_DoNotAllResolveAtTheSameRate()
        {
            var even = RunAll(Tuning.Shipped).Where(IsEvenlyMatched).ToList();

            even.Should().HaveCountGreaterThan(2);

            var original = RunAll(Tuning.Original).Where(IsEvenlyMatched).ToList();
            var originalSpread = original.Max(x => x.HitRatePercent) - original.Min(x => x.HitRatePercent);

            originalSpread.Should().BeLessThan(
                5,
                "characterizing the original tuning this harness replaced");

            var pool = new Combatant("T", 20, 350, 58, 58, 16, 18, 34, 20, 18);
            var behind = Tuning.Shipped.HitRate(pool.Evasion - 3 * ShadowrunDisplay.PoolDivisor, pool.Evasion);
            var level = Tuning.Shipped.HitRate(pool.Evasion, pool.Evasion);
            var ahead = Tuning.Shipped.HitRate(pool.Evasion + 3 * ShadowrunDisplay.PoolDivisor, pool.Evasion);

            (ahead - behind).Should().BeGreaterThanOrEqualTo(
                30,
                "a six-pool swing must change the outcome substantially, or the display is a lie " +
                "(behind {0}%, level {1}%, ahead {2}%)",
                behind,
                level,
                ahead);
        }

        /// <summary>
        /// The regression that the flat parity soak broke: an evenly matched fight at the bottom of
        /// the curve must deal damage in both directions. This is the first combat any player
        /// experiences, and it is the one a spot assertion on "weak attacks bounce off heavy armor"
        /// cannot protect.
        /// </summary>
        [Test]
        public void EveryMatchup_DealsDamage()
        {
            foreach (var r in RunAll())
            {
                r.MeanDamagePerExchange.Should().BeGreaterThan(
                    0d,
                    "combat must resolve at every tier - '{0}' deals nothing",
                    r.Label);
            }
        }

        /// <summary>
        /// Runs an actual back-and-forth duel to exhaustion, recomputing both sides' ratings as they
        /// take damage. This is the only way to see a death spiral: wound penalties are invisible in
        /// a single exchange and compound only over a whole fight.
        /// </summary>
        /// <returns>Exchanges fought, and the winner's remaining health as a percentage.</returns>
        private static (double Exchanges, double WinnerHealthPercent) Duel(
            Combatant a,
            Combatant b,
            Tuning tuning,
            bool woundsEnabled,
            int seed)
        {
            var rng = new System.Random(seed);

            var maxA = Math.Max(1, (int)Math.Round(a.HP * tuning.HealthScale));
            var maxB = Math.Max(1, (int)Math.Round(b.HP * tuning.HealthScale));
            var hpA = maxA;
            var hpB = maxB;
            var exchanges = 0;

            int Penalty(int hp, int max) =>
                woundsEnabled ? Stat.CalculateWoundPenalty(hp, max, 0) : 0;

            // Bounded so a mutual stalemate cannot hang the suite.
            while (hpA > 0 && hpB > 0 && exchanges < 5000)
            {
                exchanges++;

                Strike(a, b, hpA, maxA, hpB, maxB, ref hpB);
                if (hpB <= 0) break;

                Strike(b, a, hpB, maxB, hpA, maxA, ref hpA);
            }

            var winnerHealth = hpA > 0
                ? hpA / (double)maxA * 100d
                : hpB / (double)maxB * 100d;

            return (exchanges, winnerHealth);

            void Strike(
                Combatant atk,
                Combatant def,
                int atkHP,
                int atkMax,
                int defHP,
                int defMax,
                ref int defHPRef)
            {
                var accuracy = atk.Accuracy - Penalty(atkHP, atkMax);
                var evasion = def.Evasion - Penalty(defHP, defMax);

                if (rng.Next(1, 101) > tuning.HitRate(accuracy, evasion))
                    return;

                var (min, max) = Combat.CalculateSoakDamageRange(
                    atk.Attack, atk.DMG, atk.Stat, def.Defense, def.DefStat, 0);

                defHPRef -= max <= min ? min : rng.Next(min, max + 1);
            }
        }

        [Test]
        public void WoundPenalty_Report()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("WOUND PENALTY - duels to exhaustion, averaged over 400 seeds");
            sb.AppendLine(new string('-', 92));
            sb.AppendLine($"{"matchup",-38}{"exch off",10}{"exch on",10}{"win HP off",12}{"win HP on",11}");
            sb.AppendLine(new string('-', 92));

            foreach (var (attacker, defender, label) in Matchups().Where(m => m.Label.StartsWith("Boss") == false))
            {
                var off = Enumerable.Range(0, 400)
                    .Select(s => Duel(attacker, defender, Tuning.Shipped, false, s))
                    .ToList();
                var on = Enumerable.Range(0, 400)
                    .Select(s => Duel(attacker, defender, Tuning.Shipped, true, s))
                    .ToList();

                sb.AppendLine(
                    $"{label,-38}{off.Average(x => x.Exchanges),10:F1}{on.Average(x => x.Exchanges),10:F1}" +
                    $"{off.Average(x => x.WinnerHealthPercent),12:F1}{on.Average(x => x.WinnerHealthPercent),11:F1}");
            }

            sb.AppendLine(new string('-', 92));
            sb.AppendLine("  A large jump in winner health means injuries snowball - the death spiral.");

            TestContext.Out.WriteLine(sb.ToString());
        }

        /// <summary>
        /// The one real feel risk in the whole conversion. Wound penalties make a losing fight worse,
        /// which is the point; the failure mode is when they make it <em>hopeless</em>, turning every
        /// close fight into a rout decided by the first few rolls.
        ///
        /// Measured as how much healthier the winner walks away. A spiral shows up as the winner
        /// finishing far fresher than they would without penalties, because the loser stopped being
        /// able to fight back.
        /// </summary>
        [Test]
        public void WoundPenalty_DoesNotCauseADeathSpiral()
        {
            foreach (var (attacker, defender, label) in Matchups().Where(m => !m.Label.StartsWith("Boss")))
            {
                var off = Enumerable.Range(0, 400)
                    .Select(s => Duel(attacker, defender, Tuning.Shipped, false, s))
                    .Average(x => x.WinnerHealthPercent);
                var on = Enumerable.Range(0, 400)
                    .Select(s => Duel(attacker, defender, Tuning.Shipped, true, s))
                    .Average(x => x.WinnerHealthPercent);

                (on - off).Should().BeLessThan(
                    15d,
                    "'{0}': wound penalties should tilt a losing fight, not decide it - winner " +
                    "finishes at {1:F1}% health with them versus {2:F1}% without",
                    label,
                    on,
                    off);
            }
        }

        /// <summary>
        /// The free threshold has to actually be free, or "you are lightly wounded" silently becomes
        /// a combat penalty and players cannot tell why they started missing.
        /// </summary>
        [Test]
        public void WoundPenalty_IsFreeUntilTheThresholdIsCrossed()
        {
            Stat.CalculateWoundPenalty(100, 100, 0).Should().Be(0, "an undamaged creature is unpenalised");
            Stat.CalculateWoundPenalty(71, 100, 0).Should().Be(0, "light damage is inside the free threshold");

            Stat.CalculateWoundPenalty(1, 100, 0).Should().BeGreaterThan(
                0,
                "a nearly dead creature must be penalised");

            Stat.CalculateWoundPenalty(1, 100, 99).Should().Be(
                0,
                "a large WoundPenaltyFreeBoxes bonus must be able to cancel the penalty outright, " +
                "which is the hook cyberware like a damage compensator hangs on");
        }

        /// <summary>
        /// One die of penalty must equal exactly one displayed pool, or the character sheet and the
        /// felt outcome disagree about how hurt the player is.
        /// </summary>
        [Test]
        public void WoundPenalty_IsWholeDisplayedPools()
        {
            for (var hp = 1; hp <= 100; hp++)
            {
                var penalty = Stat.CalculateWoundPenalty(hp, 100, 0);

                (penalty % ShadowrunDisplay.PoolDivisor).Should().Be(
                    0,
                    "penalty at {0}% health was {1}, which is not a whole number of pools",
                    hp,
                    penalty);
            }
        }

        /// <summary>
        /// Guards the texture that subtractive mitigation exists to produce, phrased as a fight
        /// rather than as a formula: a hold-out weapon against heavy armor has to be able to fail
        /// outright, or armor is decorative.
        /// </summary>
        [Test]
        public void HoldOutWeapon_CannotHurtHeavyArmor_WhileHeavyWeaponCan()
        {
            var armored = new Combatant("Armored", 45, 2160, 118, 118, 21, 200, 74, 30, 30);
            var holdOut = new Combatant("Hold-out", 45, 2200, 120, 118, 30, 30, 8, 32, 30);
            var cannon = holdOut with { Name = "Assault cannon", DMG = 120 };

            Simulate(holdOut, armored, "hold-out", Tuning.Shipped).MeanDamagePerExchange.Should().Be(0d);
            Simulate(cannon, armored, "cannon", Tuning.Shipped).MeanDamagePerExchange.Should().BeGreaterThan(0d);
        }

        /// <summary>
        /// The pacing target has to hold for evenly matched fights at every tier, which is the
        /// property the original tuning missed everywhere - 14 exchanges at the low end and 42 at
        /// the high end against a target of 3-12.
        ///
        /// Overtuned matchups are excluded deliberately: a boss is supposed to be lopsided, so
        /// folding one into a pacing target would tune the wrong thing.
        /// </summary>
        [Test]
        public void EvenlyMatchedFights_ResolveAtShadowrunPace()
        {
            foreach (var r in RunAll(Tuning.Shipped).Where(IsEvenlyMatched))
            {
                r.ExchangesToKill.Should().BeInRange(
                    TargetExchangesMin,
                    TargetExchangesMax,
                    "'{0}' should resolve like a firefight, not a health bar",
                    r.Label);
            }
        }

        /// <summary>
        /// Proves the pacing target is reachable at all, and shows the cost of reaching it. Without
        /// this, a report saying "the shipped numbers are wrong" is a complaint rather than a finding.
        /// </summary>
        [Test]
        public void TuningSweep_Report()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("TUNING SWEEP - configurations landing every even matchup in target");
            sb.AppendLine(new string('-', 72));
            sb.AppendLine($"{"base%",7}{"%/pool",8}{"health",8}{"worst",9}{"best",9}{"in target",12}");
            sb.AppendLine(new string('-', 72));

            var viable = 0;

            foreach (var baseRate in new[] { 45, 50, 55, 60, 75 })
            foreach (var perPool in new[] { 4d, 8d })
            foreach (var health in new[] { 1d, 0.5d, 1d / 3d, 0.25d, 0.2d, 1d / 6d, 0.125d, 0.1d })
            {
                var tuning = new Tuning("sweep", baseRate, perPool, 5, 95, health);
                var even = RunAll(tuning).Where(IsEvenlyMatched).ToList();

                var worst = even.Max(r => r.ExchangesToKill);
                var best = even.Min(r => r.ExchangesToKill);
                var ok = even.All(r =>
                    r.ExchangesToKill >= TargetExchangesMin && r.ExchangesToKill <= TargetExchangesMax);

                if (!ok)
                    continue;

                viable++;
                sb.AppendLine(
                    $"{baseRate,7}{perPool,8:F0}{health,8:F2}{worst,9:F1}{best,9:F1}{"yes",12}");
            }

            sb.AppendLine(new string('-', 72));
            sb.AppendLine($"  {viable} viable configuration(s)");

            TestContext.Out.WriteLine(sb.ToString());

            viable.Should().BeGreaterThan(
                0,
                "if no combination of base, slope, and health curve lands evenly matched fights in " +
                "{0}-{1} exchanges, Shadowrun pacing is not reachable by tuning alone and the " +
                "conversion needs a deeper change",
                TargetExchangesMin,
                TargetExchangesMax);
        }

        /// <summary>
        /// The property the whole pool display depends on: a bigger pool has to mean a better
        /// chance, visibly. Asserted against the real engine rather than the harness model, because
        /// this is the contract the on-screen numbers make to the player.
        /// </summary>
        [Test]
        public void ShippedTuning_MakesPoolsPredictive()
        {
            var defender = new Combatant("Target", 20, 350, 58, 58, 16, 18, 34, 20, 18);

            var rates = Enumerable.Range(0, 6)
                .Select(step => Combat.CalculateHitRate(
                    defender.Evasion + step * ShadowrunDisplay.PoolDivisor,
                    defender.Evasion,
                    0))
                .ToList();

            rates.Should().BeInAscendingOrder("more dice must never mean a worse chance");

            rates[0].Should().BeInRange(45, 55, "an even pool contest should read as a coin flip");

            for (var i = 1; i < rates.Count; i++)
            {
                (rates[i] - rates[i - 1]).Should().BeGreaterThanOrEqualTo(
                    5,
                    "one pool of advantage must be visible to a player, not lost in rounding");
            }
        }
    }
}
