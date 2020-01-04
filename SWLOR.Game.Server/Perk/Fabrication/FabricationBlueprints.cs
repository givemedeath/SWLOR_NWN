using System.Collections.Generic;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWScript.Enumerations;

namespace SWLOR.Game.Server.Perk.Fabrication
{
    public class FabricationBlueprints: IPerk
    {
        public PerkType PerkType => PerkType.FabricationBlueprints;
        public string Name => "Fabrication Blueprints";
        public bool IsActive => true;
        public string Description => "Unlocks new fabrication blueprints on every odd level (1, 3, 5, 7) and adds an enhancement slot for every even level (2, 4, 6, 8) for fabrication.";
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
				1, new PerkLevel(2, "Tier 1 fabrication blueprints.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 0}, 
				})
			},
			{
				2, new PerkLevel(2, "Tier 1 fabrication blueprints. +1 enhancement slot",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 5}, 
				})
			},
			{
				3, new PerkLevel(3, "Tier 2 fabrication blueprints. +1 enhancement slot",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 10}, 
				})
			},
			{
				4, new PerkLevel(3, "Tier 2 fabrication blueprints. +2 enhancement slots",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 15}, 
				})
			},
			{
				5, new PerkLevel(4, "Tier 3 fabrication blueprints. +2 enhancement slots",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 20}, 
				})
			},
			{
				6, new PerkLevel(4, "Tier 3 fabrication blueprints. +3 enhancement slots",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 25}, 
				})
			},
			{
				7, new PerkLevel(5, "Tier 4 fabrication blueprints. +3 enhancement slots",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 30}, 
				})
			},
			{
				8, new PerkLevel(5, "Tier 4 fabrication blueprints. +4 enhancement slots",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Fabrication, 35}, 
				})
			},
		};

                public Dictionary<int, List<PerkFeat>> PerkFeats { get; } = new Dictionary<int, List<PerkFeat>>();


                public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
            
        }
    }
}
