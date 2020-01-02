using System.Collections.Generic;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWScript.Enumerations;

namespace SWLOR.Game.Server.Perk.Weaponsmith
{
    public class WeaponModInstallation: IPerk
    {
        public PerkType PerkType => PerkType.WeaponModInstallation;
        public string Name => "Weapon Mod Installation";
        public bool IsActive => true;
        public string Description => "Enables the installation of mods into weapons.";
        public PerkCategoryType Category => PerkCategoryType.Weaponsmith;
        public PerkCooldownGroup CooldownGroup => PerkCooldownGroup.None;
        public PerkExecutionType ExecutionType => PerkExecutionType.None;
        public bool IsTargetSelfOnly => false;
        public int Enmity => 0;
        public EnmityAdjustmentRuleType EnmityAdjustmentType => EnmityAdjustmentRuleType.None;
        public ForceBalanceType ForceBalanceType => ForceBalanceType.Universal;
        public Animation CastAnimation => Animation.Invalid;

        public string CanCastSpell(NWCreature oPC, NWObject oTarget, int spellTier)
        {
            return string.Empty;
        }
        
        public int FPCost(NWCreature oPC, int baseFPCost, int spellTier)
        {
            return baseFPCost;
        }

        public float CastingTime(NWCreature oPC, int spellTier)
        {
            return 0f;
        }

        public float CooldownTime(NWCreature oPC, float baseCooldownTime, int spellTier)
        {
            return baseCooldownTime;
        }

        public void OnImpact(NWCreature creature, NWObject target, int perkLevel, int spellTier)
        {
        }

        public void OnPurchased(NWCreature creature, int newLevel)
        {
        }

        public void OnRemoved(NWCreature creature)
        {
        }

        public void OnItemEquipped(NWCreature creature, NWItem oItem)
        {
        }

        public void OnItemUnequipped(NWCreature creature, NWItem oItem)
        {
        }

        public void OnCustomEnmityRule(NWCreature creature, int amount)
        {
        }

        public bool IsHostile()
        {
            return false;
        }

        		public Dictionary<int, PerkLevel> PerkLevels => new Dictionary<int, PerkLevel>
		{
			{
				1, new PerkLevel(2, "Install mods up to level 5 on items up to level 10.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 5}, 
				})
			},
			{
				2, new PerkLevel(2, "Install mods up to level 10 on items up to level 20.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 10}, 
				})
			},
			{
				3, new PerkLevel(2, "Install mods up to level 15 on items up to level 30.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 15}, 
				})
			},
			{
				4, new PerkLevel(3, "Install mods up to level 20 on items up to level 40.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 20}, 
				})
			},
			{
				5, new PerkLevel(3, "Install mods up to level 25 on items up to level 50.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 25}, 
				})
			},
			{
				6, new PerkLevel(4, "Install mods up to level 30 on items up to level 60.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 30}, 
				})
			},
			{
				7, new PerkLevel(4, "Install mods up to level 35 on items up to level 70.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 35}, 
				})
			},
			{
				8, new PerkLevel(5, "Install mods up to level 40 on items up to level 80.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 40}, 
				})
			},
			{
				9, new PerkLevel(5, "Install mods up to level 45 on items up to level 90.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 45}, 
				})
			},
			{
				10, new PerkLevel(5, "Install mods up to level 50 on items up to level 100.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Weaponsmith, 50}, 
				})
			},
		};


        public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
            
        }
    }
}
