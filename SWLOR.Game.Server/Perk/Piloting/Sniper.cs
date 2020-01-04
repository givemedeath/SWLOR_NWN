using System.Collections.Generic;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWScript.Enumerations;

namespace SWLOR.Game.Server.Perk.Piloting
{
    public class Sniper: IPerk
    {
        public PerkType PerkType => PerkType.Sniper;
        public string Name => "Sniper";
        public bool IsActive => true;
        public string Description => "When acting as a gunner, increases the range of the ship.";
        public PerkCategoryType Category => PerkCategoryType.Piloting;
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
				1, new PerkLevel(2, "Increases range of the ship by 1.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Piloting, 50}, 
				})
			},
			{
				2, new PerkLevel(2, "Increases range of the ship by 2.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Piloting, 60}, 
				})
			},
			{
				3, new PerkLevel(3, "Increases range of the ship by 3.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Piloting, 70}, 
				})
			},
			{
				4, new PerkLevel(3, "Increases range of the ship by 4.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Piloting, 80}, 
				})
			},
			{
				5, new PerkLevel(4, "Increases range of the ship by 5.",
				new Dictionary<SkillType, int>
				{
					{ SkillType.Piloting, 90}, 
				})
			},
		};

                public Dictionary<int, List<PerkFeat>> PerkFeats { get; } = new Dictionary<int, List<PerkFeat>>();


                public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
            
        }
    }
}
