using System.Collections.Generic;
using SWLOR.Game.Server.Service.SkillService;
using SWLOR.Game.Server.Service.StatService;

namespace SWLOR.Game.Server.Service.CyberwareService
{
    /// <summary>
    /// Fluent builder for cyberware, modelled on <see cref="SpaceService.ShipModuleBuilder"/>. Keyed
    /// by a stable definition id so installed cyberware can be looked up from the id stored on the
    /// player.
    /// </summary>
    public class CyberwareBuilder
    {
        private readonly Dictionary<string, CyberwareDetail> _cyberware = new();
        private CyberwareDetail _active;

        /// <summary>Begins a new cyberware definition with a stable id.</summary>
        public CyberwareBuilder Create(string id)
        {
            _active = new CyberwareDetail();
            _cyberware[id] = _active;

            return this;
        }

        public CyberwareBuilder Name(string name)
        {
            _active.Name = name;
            return this;
        }

        public CyberwareBuilder ShortName(string shortName)
        {
            _active.ShortName = shortName;
            return this;
        }

        public CyberwareBuilder Description(string description)
        {
            _active.Description = description;
            return this;
        }

        public CyberwareBuilder EssenceCost(float essenceCost)
        {
            _active.EssenceCost = essenceCost;
            return this;
        }

        public CyberwareBuilder Price(int nuyen)
        {
            _active.InstallPrice = nuyen;
            return this;
        }

        /// <summary>Declares a passive stat bonus granted while this piece is installed.</summary>
        public CyberwareBuilder IncreasesStat(StatType stat, int amount)
        {
            _active.StatBonuses[stat] = amount;
            return this;
        }

        public CyberwareBuilder RequirementSkill(SkillType skill, int rank)
        {
            _active.RequiredSkill = skill;
            _active.RequiredSkillRank = rank;
            return this;
        }

        public Dictionary<string, CyberwareDetail> Build()
        {
            return _cyberware;
        }
    }
}
