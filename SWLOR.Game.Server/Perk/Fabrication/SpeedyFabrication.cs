using System.Collections.Generic;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWScript.Enumerations;

namespace SWLOR.Game.Server.Perk.Fabrication
{
    public class SpeedyFabrication: IPerk
    {
        public PerkType PerkType => PerkType.SpeedyFabrication;
        public string Name => "Speedy Fabrication";
        public bool IsActive => true;
        public string Description => "Reduces the amount of time it takes to craft fabrication items.";
        public PerkCategoryType Category => PerkCategoryType.Fabrication;
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
				1, new PerkLevel(2, "+10% Crafting Speed",
				new Dictionary<SkillType, int>
				{

				})
			},
			{
				2, new PerkLevel(2, "+20% Crafting Speed",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 5}, 
				})
			},
			{
				3, new PerkLevel(3, "+30% Crafting Speed",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 10}, 
				})
			},
			{
				4, new PerkLevel(3, "+40% Crafting Speed",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 15}, 
				})
			},
			{
				5, new PerkLevel(3, "+50% Crafting Speed",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 20}, 
				})
			},
			{
				6, new PerkLevel(4, "+60% Crafting Speed",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 25}, 
				})
			},
			{
				7, new PerkLevel(4, "+70% Crafting Speed",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 30}, 
				})
			},
			{
				8, new PerkLevel(4, "+80% Crafting Speed",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 35}, 
				})
			},
			{
				9, new PerkLevel(5, "+90% Crafting Speed",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 40}, 
				})
			},
			{
				10, new PerkLevel(6, "+99% Crafting Speed",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 50}, 
				})
			},
		};


        public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
            
        }
    }
}
