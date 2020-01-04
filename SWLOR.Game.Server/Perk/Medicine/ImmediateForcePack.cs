using System.Collections.Generic;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWScript.Enumerations;

namespace SWLOR.Game.Server.Perk.Medicine
{
    public class ImmediateForcePack: IPerk
    {
        public PerkType PerkType => PerkType.ImmediateForcePack;
        public string Name => "Immediate Force Pack";
        public bool IsActive => true;
        public string Description => "Force packs immediately recover FP in addition to their natural recovery over time.";
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
				1, new PerkLevel(2, "1x Force Pack Effectiveness",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Medicine, 5}, 
				})
			},
			{
				2, new PerkLevel(2, "2x Force Pack Effectiveness",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Medicine, 10}, 
				})
			},
			{
				3, new PerkLevel(3, "3x Force Pack Effectiveness",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Medicine, 15}, 
				})
			},
			{
				4, new PerkLevel(3, "4x Force Pack Effectiveness",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Medicine, 20}, 
				})
			},
		};

                public Dictionary<int, List<PerkFeat>> PerkFeats { get; } = new Dictionary<int, List<PerkFeat>>();


                public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
            
        }
    }
}
