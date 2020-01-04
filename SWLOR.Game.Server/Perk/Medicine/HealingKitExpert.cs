using System.Collections.Generic;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWScript.Enumerations;

namespace SWLOR.Game.Server.Perk.Medicine
{
    public class HealingKitExpert : IPerk
    {
        public PerkType PerkType => PerkType.HealingKitExpert;
        public string Name => "Healing Kit Expert";
        public bool IsActive => true;
        public string Description => "Improves the duration of healing kits.";
        public PerkCategoryType Category => PerkCategoryType.Medicine;
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
				1, new PerkLevel(2, "+6 Seconds",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Medicine, 10}, 
				})
			},
			{
				2, new PerkLevel(2, "+12 Seconds",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Medicine, 20}, 
				})
			},
			{
				3, new PerkLevel(3, "+18 Seconds",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Medicine, 30}, 
				})
			},
			{
				4, new PerkLevel(3, "+24 Seconds",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Medicine, 40}, 
				})
			},
			{
				5, new PerkLevel(4, "+30 Seconds",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Medicine, 50}, 
				})
			},
		};

                public Dictionary<int, List<PerkFeat>> PerkFeats { get; } = new Dictionary<int, List<PerkFeat>>();


                public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
            
        }
    }
}
