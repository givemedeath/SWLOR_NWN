using System;
using System.Collections.Generic;
using System.Linq;
using SWLOR.Game.Server.Core;
using SWLOR.Game.Server.Entity;
using SWLOR.Game.Server.Service.CyberwareService;
using SWLOR.Game.Server.Service.SkillService;
using SWLOR.Game.Server.Service.StatService;

namespace SWLOR.Game.Server.Service
{
    /// <summary>
    /// Cyberware and Essence - the chrome-versus-magic tradeoff at the heart of the setting.
    ///
    /// Modelled on the ship-module system (a socketable-modules-with-capacity design) but wired
    /// through the player stat layer rather than a status struct, per decision D16. Passive stat
    /// grants are declared on each <see cref="CyberwareDetail"/> and read live through
    /// <see cref="GetStatBonus"/>; Essence is a 0-6 budget tracked on the player; installing chrome
    /// reduces Magic for everyone through the single <see cref="Stat.GetMaxFP(uint, Player)"/>
    /// chokepoint.
    /// </summary>
    public static class Cyberware
    {
        /// <summary>The full Essence budget. Cyberware Essence costs are subtracted from this.</summary>
        public const float MaxEssence = 6.0f;

        private static readonly Dictionary<string, CyberwareDetail> _cyberware = new();

        /// <summary>
        /// Every stat any cyberware grants a bonus to. Lets <see cref="GetStatBonus"/> short-circuit
        /// before any engine or database call for unrelated stats - the guard
        /// <see cref="Mimicry.GetStatBonus"/> uses, and what keeps the shared stat read cheap and
        /// unit-testable.
        /// </summary>
        private static readonly HashSet<StatType> _affectedStats = new();

        /// <summary>Reflection-discovers every <see cref="ICyberwareListDefinition"/> at module load.</summary>
        [NWNEventHandler(ScriptName.OnModuleCacheBefore)]
        public static void CacheData()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(w => typeof(ICyberwareListDefinition).IsAssignableFrom(w) && !w.IsInterface && !w.IsAbstract);

            foreach (var type in types)
            {
                var instance = (ICyberwareListDefinition)Activator.CreateInstance(type);
                foreach (var (id, detail) in instance.BuildCyberware())
                {
                    _cyberware[id] = detail;
                    foreach (var stat in detail.StatBonuses.Keys)
                        _affectedStats.Add(stat);
                }
            }
        }

        public static IReadOnlyDictionary<string, CyberwareDetail> GetAll()
        {
            return _cyberware;
        }

        public static bool Exists(string id)
        {
            return _cyberware.ContainsKey(id);
        }

        public static CyberwareDetail GetById(string id)
        {
            return _cyberware.TryGetValue(id, out var detail) ? detail : null;
        }

        // ---- Essence ----------------------------------------------------------------------------

        /// <summary>Essence still available to spend on new cyberware.</summary>
        public static float GetEssenceAvailable(Player dbPlayer)
        {
            return MaxEssence - dbPlayer.EssenceSpent;
        }

        /// <summary>
        /// Recomputes Essence spent from the installed list. The player's <c>EssenceSpent</c> is a
        /// cache maintained on install and removal; this is the authoritative recomputation used to
        /// set it and available for repair.
        /// </summary>
        public static float CalculateEssenceSpent(Player dbPlayer)
        {
            return dbPlayer.InstalledCyberware
                .Select(GetById)
                .Where(detail => detail != null)
                .Sum(detail => detail.EssenceCost);
        }

        // ---- Stat grants ------------------------------------------------------------------------

        /// <summary>
        /// The total bonus installed cyberware grants to a stat, folded into
        /// <see cref="Stat.GetStatAdjustmentExcludingTemporaryModifiers"/>. Short-circuits before any
        /// engine or database call for stats no cyberware affects.
        /// </summary>
        public static int GetStatBonus(uint creature, StatType stat)
        {
            if (!_affectedStats.Contains(stat))
                return 0;

            if (!GetIsPC(creature) || GetIsDM(creature))
                return 0;

            var dbPlayer = DB.Get<Player>(GetObjectUUID(creature));
            if (dbPlayer == null)
                return 0;

            return GetStatBonus(dbPlayer, stat);
        }

        /// <summary>Pure overload: the stat bonus for a player's installed cyberware, testable without a server.</summary>
        public static int GetStatBonus(Player dbPlayer, StatType stat)
        {
            var bonus = 0;
            foreach (var id in dbPlayer.InstalledCyberware)
            {
                var detail = GetById(id);
                if (detail != null && detail.StatBonuses.TryGetValue(stat, out var amount))
                    bonus += amount;
            }

            return bonus;
        }

        // ---- Install / remove -------------------------------------------------------------------

        /// <summary>
        /// Whether a piece can be installed: it exists, is not already installed, fits the remaining
        /// Essence budget, and the skill gate is met. Takes the skill rank as a parameter so the rule
        /// is testable without a creature. Returns an empty string on success, or the reason it fails.
        /// </summary>
        public static string GetInstallBlockReason(Player dbPlayer, string id, int skillRank)
        {
            var detail = GetById(id);
            if (detail == null)
                return "That cyberware does not exist.";

            if (dbPlayer.InstalledCyberware.Contains(id))
                return "That cyberware is already installed.";

            if (detail.EssenceCost > GetEssenceAvailable(dbPlayer) + 0.0001f)
                return "You do not have enough Essence remaining.";

            if (detail.RequiredSkill != SkillType.Invalid && skillRank < detail.RequiredSkillRank)
                return $"This requires {detail.RequiredSkill} rank {detail.RequiredSkillRank}.";

            return string.Empty;
        }

        /// <summary>
        /// Records a piece as installed and updates the cached Essence spent. Does not touch gold or
        /// the creature - the caller (the clinic UI) handles payment and the post-change FP refresh.
        /// </summary>
        public static void AddInstalled(Player dbPlayer, string id)
        {
            if (!dbPlayer.InstalledCyberware.Contains(id))
                dbPlayer.InstalledCyberware.Add(id);

            dbPlayer.EssenceSpent = CalculateEssenceSpent(dbPlayer);
        }

        /// <summary>Records a piece as removed and updates the cached Essence spent.</summary>
        public static void RemoveInstalled(Player dbPlayer, string id)
        {
            dbPlayer.InstalledCyberware.Remove(id);
            dbPlayer.EssenceSpent = CalculateEssenceSpent(dbPlayer);
        }
    }
}
