using System.Collections.Generic;
using SWLOR.Game.Server.Service.SkillService;
using SWLOR.Game.Server.Service.StatService;

namespace SWLOR.Game.Server.Service.CyberwareService
{
    /// <summary>
    /// One installable piece of cyberware.
    ///
    /// Passive stat grants are <b>declarative</b> - a map of <see cref="StatType"/> to amount read
    /// live through <see cref="Cyberware.GetStatBonus"/> and folded into the shared player stat layer,
    /// rather than an equip/unequip mutation of a status struct the way ship modules work. That is the
    /// deliberate departure recorded in decision D16: personal combat reads the stat-adjustment layer,
    /// not a per-object status struct, so cyberware must feed the former.
    /// </summary>
    public class CyberwareDetail
    {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>Essence this piece consumes from the 0-6 budget. Fractional.</summary>
        public float EssenceCost { get; set; }

        /// <summary>Cost to install, in nuyen (NWN gold).</summary>
        public int InstallPrice { get; set; }

        /// <summary>Passive stat bonuses granted while installed.</summary>
        public Dictionary<StatType, int> StatBonuses { get; set; } = new();

        /// <summary>Optional skill gate: minimum rank required to install.</summary>
        public SkillType RequiredSkill { get; set; } = SkillType.Invalid;
        public int RequiredSkillRank { get; set; }
    }
}
