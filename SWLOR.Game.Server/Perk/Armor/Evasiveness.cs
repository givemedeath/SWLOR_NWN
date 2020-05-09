﻿using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;

using NWN;
using SWLOR.Game.Server.NWN;
using NWScript = SWLOR.Game.Server.NWN.NWScript;


namespace SWLOR.Game.Server.Perk.Armor
{
    public class Evasiveness : IPerkHandler
    {
        public PerkType PerkType => PerkType.Evasiveness;

        public string CanCastSpell(NWCreature oPC, NWObject oTarget, int spellTier)
        {
            NWItem armor = oPC.Chest;
            if (armor.CustomItemType != CustomItemType.LightArmor)
                return "You must be equipped with light armor to use that ability.";

            return string.Empty;
        }

        public int FPCost(NWCreature oPC, int baseFPCost, int spellTier)
        {
            return baseFPCost;
        }

        public float CastingTime(NWCreature oPC, float baseCastingTime, int spellTier)
        {
            return baseCastingTime;
        }

        public float CooldownTime(NWCreature oPC, float baseCooldownTime, int spellTier)
        {
            return baseCooldownTime;
        }

        public int? CooldownCategoryID(NWCreature creature, int? baseCooldownCategoryID, int spellTier)
        {
            return baseCooldownCategoryID;
        }

        public void OnImpact(NWCreature creature, NWObject target, int perkLevel, int spellTier)
        {
            int concealment;
            float length;

            switch (perkLevel)
            {
                case 1:
                    concealment = 10;
                    length = 12.0f;
                    break;
                case 2:
                    concealment = 15;
                    length = 12.0f;
                    break;
                case 3:
                    concealment = 20;
                    length = 12.0f;
                    break;
                case 4:
                    concealment = 25;
                    length = 12.0f;
                    break;
                case 5:
                    concealment = 30;
                    length = 18.0f;
                    break;
                default:
                    return;
            }

            Effect effect = NWScript.EffectConcealment(concealment);
            NWScript.ApplyEffectToObject(DurationType.Temporary, effect, creature.Object, length);

            effect = NWScript.EffectVisualEffect(NWScript.VFX_DUR_AURA_CYAN);
            NWScript.ApplyEffectToObject(DurationType.Temporary, effect, creature.Object, length);

            effect = NWScript.EffectVisualEffect(NWScript.VFX_IMP_AC_BONUS);
            NWScript.ApplyEffectToObject(DurationType.Instant, effect, creature.Object);
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

        public void OnConcentrationTick(NWCreature creature, NWObject target, int perkLevel, int tick)
        {
            
        }
    }
}
