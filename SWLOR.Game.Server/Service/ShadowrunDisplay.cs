using System;

namespace SWLOR.Game.Server.Service
{
    /// <summary>
    /// Translates SWLOR's internal combat values into the Shadowrun vocabulary shown to players.
    ///
    /// This is a presentation layer and nothing more: it reads values, it never changes them. The
    /// underlying resolution math (accuracy versus evasion, hit rate, damage) is untouched and lives
    /// in <see cref="Stat"/> and <see cref="Combat"/>.
    ///
    /// The translation is coherent rather than arbitrary because the inputs already have the right
    /// shape. Accuracy and Evasion are both built as <c>8 + 2*rank + attribute + gear</c>, which is
    /// exactly how a Shadowrun dice pool is composed, and the engine already resolves attacks by
    /// subtracting one from the other. Only the presentation of that difference changes here.
    /// </summary>
    public static class ShadowrunDisplay
    {
        /// <summary>
        /// Divisor converting an Accuracy or Evasion rating into a displayed dice pool.
        ///
        /// Calibrated against the band where the engine actually behaves differently: hit rate is
        /// <c>75 + floor((ACC - EVA)/2)</c> clamped to [20, 95], so the difference only matters
        /// across roughly [-110, +40]. At this divisor, player-range ratings land on pools of about
        /// 1 to 18. See decision D4.
        ///
        /// This is the single knob governing how the whole game reads. Expect to retune it once
        /// characters exist at level cap.
        /// </summary>
        public const int PoolDivisor = 8;

        /// <summary>
        /// The constant base term shared by every Accuracy and Evasion rating, removed before
        /// dividing so a minimum-rating character starts near a pool of zero rather than one.
        /// </summary>
        public const int PoolBaseOffset = 8;

        /// <summary>
        /// Boxes in a condition monitor. Shadowrun derives this from Body or Willpower (giving
        /// roughly 9 to 12); a fixed count is used here because the monitor is rendered from a
        /// percentage of an existing bar rather than from a tracked box count.
        /// </summary>
        public const int ConditionMonitorBoxes = 10;

        /// <summary>Divisor converting a skill rank (capped at 50 for most skills) to a 1-7 rating.</summary>
        public const int SkillRatingDivisor = 8;

        /// <summary>Divisor converting an attribute score to a Shadowrun-scale attribute rating.</summary>
        public const int AttributeRatingDivisor = 5;

        /// <summary>
        /// Converts a rating built on the <c>8 + 2*rank + attribute + gear</c> shape into a dice pool.
        ///
        /// Floored at zero because a negative dice pool is meaningless, while zero dice is
        /// meaningful - it reads as automatic failure.
        ///
        /// Deliberately has no upper bound. Strong NPCs and bosses are overtuned well past anything a
        /// player reaches, and their stats come from an uncapped stat skin. Clamping would render a
        /// boss identically to a strong player, hiding threat exactly when it matters most, and would
        /// break the monotonicity this class guarantees by collapsing distinct ratings onto one
        /// displayed value. Large pools are also setting-correct: Shadowrun's apex threats genuinely
        /// roll far more dice than any runner. See decision D5.
        /// </summary>
        private static int ToPool(int rating)
        {
            var pool = (int)Math.Round(
                (rating - PoolBaseOffset) / (double)PoolDivisor,
                MidpointRounding.AwayFromZero);

            return Math.Max(0, pool);
        }

        /// <summary>The attacker's dice pool, displayed in place of an Accuracy rating.</summary>
        public static int GetAttackPool(int accuracy)
        {
            return ToPool(accuracy);
        }

        /// <summary>
        /// The defender's dice pool, displayed in place of an Evasion rating.
        ///
        /// Kept separate from <see cref="GetAttackPool"/> despite identical arithmetic: callers
        /// reference these by gameplay meaning, and the two are free to diverge later without
        /// touching every call site.
        /// </summary>
        public static int GetDefensePool(int evasion)
        {
            return ToPool(evasion);
        }

        /// <summary>
        /// Formats an opposed test for the combat log, replacing the percentage form.
        ///
        /// Takes ratings rather than pools so callers pass what they already hold and cannot
        /// accidentally convert twice.
        /// </summary>
        public static string FormatOpposedTest(int accuracy, int evasion)
        {
            return $"(Pool {GetAttackPool(accuracy)} vs {GetDefensePool(evasion)})";
        }

        /// <summary>
        /// Damage Value, displayed in place of a weapon's damage rating. Direct: the concept is the
        /// same in both systems, only the name differs.
        /// </summary>
        public static int GetDamageValue(int weaponDamage)
        {
            return Math.Max(0, weaponDamage);
        }

        /// <summary>
        /// Undamaged boxes remaining on a condition monitor, from a current/maximum pair.
        ///
        /// Proportional, so it needs no special handling for overtuned enemies: a boss with fifty
        /// thousand hit points shows the same box count as a player, which is correct because the
        /// monitor communicates how hurt something is, not how much health it has.
        /// </summary>
        private static int ToConditionBoxes(int current, int maximum)
        {
            if (maximum <= 0 || current <= 0)
                return 0;

            var ratio = Math.Min(1d, current / (double)maximum);

            return (int)Math.Ceiling(ratio * ConditionMonitorBoxes);
        }

        /// <summary>Physical condition monitor, displayed in place of hit points.</summary>
        public static int GetPhysicalConditionBoxes(int currentHP, int maxHP)
        {
            return ToConditionBoxes(currentHP, maxHP);
        }

        /// <summary>
        /// Stun condition monitor, displayed in place of stamina.
        ///
        /// Stamina doubling as both an ability cost and the stun track reads plausibly at a glance:
        /// in Shadowrun, spellcasting Drain is stun damage, and physical exertion filling the same
        /// track suits non-casters.
        ///
        /// <para><b>Provisional, and the resemblance does not survive contact.</b> A real stun track
        /// is <em>filled</em> by damage; SWLOR's stamina is <em>spent</em> on abilities. D11 had to
        /// exclude this track from wound penalties for exactly that reason — charging them would
        /// punish players for using abilities rather than for being hurt. Expect this mapping to be
        /// replaced or removed. See "Deferred — P9" in <c>design/shadowrun/PLAN.md</c>.</para>
        /// </summary>
        public static int GetStunConditionBoxes(int currentStamina, int maxStamina)
        {
            return ToConditionBoxes(currentStamina, maxStamina);
        }

        /// <summary>
        /// The Magic pool, displayed in place of Force Points. Direct and deliberately ungraduated.
        ///
        /// This is explicitly *not* Edge. Shadowrun's Edge is a 1-7 luck attribute spent on rerolls,
        /// while this is a resource pool drained by ability costs of roughly two to nine at a time.
        /// Compressing it onto an Edge-sized scale would hide most individual spends - measured at 44%
        /// of casts invisible at a 60 pool and 79% at 150 - which is unacceptable for a gauge players
        /// read mid-fight. A resource bar has to resolve every spend.
        ///
        /// Leaving the Edge name unused also keeps it available for a real Edge mechanic, which pairs
        /// naturally with spending Edge to reroll a glitch. See decision D6.
        ///
        /// <para><b>Provisional.</b> This is the honest way to display a resource bar, but Shadowrun
        /// magic is not a bar: Magic is an attribute rating, and casting costs Drain — damage landing
        /// on the stun monitor — rather than depletion. Expect this mapping to be replaced or removed
        /// rather than refined. See "Deferred — P9" in <c>design/shadowrun/PLAN.md</c>.</para>
        /// </summary>
        public static int GetMagicPool(int forcePoints)
        {
            return Math.Max(0, forcePoints);
        }

        /// <summary>
        /// Shadowrun skill rating, displayed in place of a skill rank. Player ranks cap at 50 for most
        /// skills, giving 1-7; NPC ranks may exceed that and are intentionally not clamped.
        /// </summary>
        public static int GetSkillRating(int rank)
        {
            if (rank <= 0)
                return 0;

            return (int)Math.Ceiling(rank / (double)SkillRatingDivisor);
        }

        /// <summary>
        /// Shadowrun attribute rating, displayed in place of a raw attribute score. Unclamped above
        /// for the same reason as pools.
        /// </summary>
        public static int GetAttributeRating(int score)
        {
            if (score <= 0)
                return 0;

            return (int)Math.Ceiling(score / (double)AttributeRatingDivisor);
        }
    }
}
