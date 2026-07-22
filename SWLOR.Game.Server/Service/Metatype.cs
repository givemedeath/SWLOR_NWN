using System;
using System.Collections.Generic;
using SWLOR.Game.Server.Core;
using SWLOR.Game.Server.Service.StatService;
using SWLOR.NWN.API.Engine;
using SWLOR.NWN.API.NWScript.Enum;

namespace SWLOR.Game.Server.Service
{
    /// <summary>
    /// Shadowrun metatype identity: the attribute modifiers and signature trait each of the five
    /// metatypes carries. Kept separate from <see cref="Race"/>, which owns appearance data for every
    /// species; this service owns only the Shadowrun-specific mechanical identity.
    ///
    /// Three mechanisms, each chosen to avoid the raw-ability-score plumbing that would otherwise fight
    /// the AP economy and the character rebuild:
    ///
    /// - <b>Attribute modifiers</b> apply as permanent <c>EffectAbilityIncrease</c>/<c>Decrease</c>
    ///   effects, reapplied on every login. This is how SWLOR already applies ability modifiers
    ///   (see <see cref="StatusEffect"/>), and it layers on top of the base score rather than mutating
    ///   it — so the rebuild's "score &lt;= 10" validation, which reads the base, is untouched, and
    ///   negative metatype modifiers never corrupt a stored value.
    /// - <b>Signature traits</b> (troll dermal armor, dwarf toxin resistance) apply as
    ///   <see cref="StatType"/> bonuses read through <see cref="GetStatBonus"/>, which
    ///   <see cref="Stat.GetStatAdjustmentExcludingTemporaryModifiers"/> folds in alongside perk and
    ///   status bonuses. No effect to manage, no stacking risk.
    /// - <b>Vision</b> applies as a permanent <c>EffectUltravision</c> on the same login pass.
    ///
    /// The numbers are a starting proposal, not derived from the tabletop. Per the project's working
    /// method they are tuned in playtest; see decision D15.
    /// </summary>
    public static class Metatype
    {
        /// <summary>
        /// Tag shared by every effect this service applies, so a login pass can clear the previous
        /// pass before reapplying. Without this, permanent ability effects would stack on every login.
        /// </summary>
        private const string MetatypeEffectTag = "SR_METATYPE_TRAIT";

        /// <summary>Condition-monitor-independent floor for an ability score after negative modifiers.</summary>
        private const int MinimumAdjustedAbilityScore = 3;

        private sealed class MetatypeProfile
        {
            public Dictionary<AbilityType, int> AttributeModifiers { get; init; } = new();
            public Dictionary<StatType, int> TraitBonuses { get; init; } = new();
            public bool HasLowLightVision { get; init; }
        }

        // SWLOR attribute mapping: Might = melee/Strength, Perception = ranged, Vitality = Body,
        // Agility = reflexes/evasion, Willpower, Social = Charisma.
        private static readonly Dictionary<RacialType, MetatypeProfile> _profiles = new()
        {
            // Humans are the flexible baseline: no modifiers, no signature trait. The reserved Edge
            // mechanic (decision D6) is their eventual identity.
            [RacialType.Human] = new MetatypeProfile(),

            // Elf: agile and personable, keen-eyed in the dark.
            [RacialType.Elf] = new MetatypeProfile
            {
                AttributeModifiers = { [AbilityType.Agility] = 1, [AbilityType.Social] = 1 },
                HasLowLightVision = true,
            },

            // Dwarf: hardy and strong-willed, and famously hard to poison.
            [RacialType.Dwarf] = new MetatypeProfile
            {
                AttributeModifiers = { [AbilityType.Vitality] = 1, [AbilityType.Willpower] = 1 },
                TraitBonuses = { [StatType.PoisonDefense] = 5 },
            },

            // Ork: tough and strong, low-light vision.
            [RacialType.Halforc] = new MetatypeProfile
            {
                AttributeModifiers = { [AbilityType.Vitality] = 1, [AbilityType.Might] = 1 },
                HasLowLightVision = true,
            },

            // Troll: enormously strong and durable, dermal armor, but slow and off-putting.
            [RacialType.Troll] = new MetatypeProfile
            {
                AttributeModifiers =
                {
                    [AbilityType.Vitality] = 2,
                    [AbilityType.Might] = 2,
                    [AbilityType.Agility] = -1,
                    [AbilityType.Social] = -1,
                },
                // Flat Defense flows straight into the subtractive soak (decisions D3/D8): a troll
                // literally shrugs off light hits.
                TraitBonuses = { [StatType.Defense] = 3 },
                HasLowLightVision = true,
            },
        };

        /// <summary>
        /// Every stat any metatype grants a signature-trait bonus to. Used to short-circuit
        /// <see cref="GetStatBonus"/> before it touches the engine, so a query for an unrelated stat
        /// never calls an NWScript function - the same guard <see cref="Mimicry.GetStatBonus"/> uses,
        /// which also keeps the aggregate stat read unit-testable.
        /// </summary>
        private static readonly HashSet<StatType> _traitStats = BuildTraitStatSet();

        private static HashSet<StatType> BuildTraitStatSet()
        {
            var stats = new HashSet<StatType>();
            foreach (var profile in _profiles.Values)
                foreach (var stat in profile.TraitBonuses.Keys)
                    stats.Add(stat);

            return stats;
        }

        /// <summary>True if the racial type is one of the five playable Shadowrun metatypes.</summary>
        public static bool IsMetatype(RacialType race)
        {
            return _profiles.ContainsKey(race);
        }

        /// <summary>The five playable metatypes, for tests and UI.</summary>
        public static IReadOnlyCollection<RacialType> Metatypes => _profiles.Keys;

        /// <summary>The metatype's attribute modifiers, or an empty map for a non-metatype race.</summary>
        public static IReadOnlyDictionary<AbilityType, int> GetAttributeModifiers(RacialType race)
        {
            return _profiles.TryGetValue(race, out var profile)
                ? profile.AttributeModifiers
                : new Dictionary<AbilityType, int>();
        }

        /// <summary>
        /// The metatype signature-trait bonus for a stat, read by
        /// <see cref="Stat.GetStatAdjustmentExcludingTemporaryModifiers"/>. Mirrors
        /// <see cref="Perk.GetStatBonus"/> so shared systems read the value instead of special-casing
        /// a race, per the AGENTS stat-driven rule.
        /// </summary>
        public static int GetStatBonus(uint creature, StatType stat)
        {
            // Short-circuit before any engine call for stats no metatype touches. This is what keeps
            // the shared stat read in Stat.GetStatAdjustmentExcludingTemporaryModifiers callable
            // without an initialised engine, and it means the common case does no work.
            if (!_traitStats.Contains(stat))
                return 0;

            if (!GetIsPC(creature) || GetIsDM(creature))
                return 0;

            return GetTraitBonus(GetRacialType(creature), stat);
        }

        /// <summary>
        /// The signature-trait bonus a metatype grants to a stat, independent of any creature. Pure,
        /// so the metatype identity is testable without a running server.
        /// </summary>
        public static int GetTraitBonus(RacialType race, StatType stat)
        {
            if (!_profiles.TryGetValue(race, out var profile))
                return 0;

            return profile.TraitBonuses.TryGetValue(stat, out var bonus) ? bonus : 0;
        }

        /// <summary>
        /// Reapplies a player's metatype attribute and vision effects on entry.
        ///
        /// Attribute and vision effects are permanent but do not persist across sessions, so they are
        /// rebuilt each login. Previously applied metatype effects are cleared first by tag so the
        /// permanent ability effects do not stack. Signature stat traits are not applied here — they
        /// are read live through <see cref="GetStatBonus"/>.
        /// </summary>
        [NWNEventHandler(ScriptName.OnModuleEnter)]
        public static void ApplyMetatypeEffects()
        {
            var player = GetEnteringObject();
            if (!GetIsPC(player) || GetIsDM(player) || GetIsDMPossessed(player))
                return;

            var race = GetRacialType(player);
            if (!_profiles.TryGetValue(race, out var profile))
                return;

            ClearMetatypeEffects(player);

            foreach (var (ability, amount) in profile.AttributeModifiers)
            {
                if (amount == 0)
                    continue;

                var effect = amount > 0
                    ? EffectAbilityIncrease(ability, amount)
                    : EffectAbilityDecrease(ability, Math.Abs(amount));

                Apply(player, effect);
            }

            if (profile.HasLowLightVision)
                Apply(player, EffectUltravision());
        }

        private static void Apply(uint player, Effect effect)
        {
            effect = TagEffect(SupernaturalEffect(effect), MetatypeEffectTag);
            ApplyEffectToObject(DurationType.Permanent, effect, player);
        }

        private static void ClearMetatypeEffects(uint player)
        {
            for (var effect = GetFirstEffect(player); GetIsEffectValid(effect); effect = GetNextEffect(player))
            {
                if (GetEffectTag(effect) == MetatypeEffectTag)
                    RemoveEffect(player, effect);
            }
        }
    }
}
