﻿using System.Linq;
using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Item.Contracts;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.Service;

using SWLOR.Game.Server.ValueObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Item.Medicine
{
    public class StimPack: IActionItem
    {
        public string CustomKey => "Medicine.StimPack";

        public CustomData StartUseItem(NWCreature user, NWItem item, NWObject target, Location targetLocation)
        {
            return null;
        }

        public void ApplyEffects(NWCreature user, NWItem item, NWObject target, Location targetLocation, CustomData customData)
        {
            if (target.ObjectType != ObjectType.Creature)
            {
                user.SendMessage("You may only use stim packs on creatures!");
                return;
            }

            NWPlayer player = user.Object;
            int ability = item.GetLocalInt("ABILITY_TYPE");
            int amount = item.GetLocalInt("AMOUNT") + item.MedicineBonus;
            int rank = player.IsPlayer ? SkillService.GetPCSkillRank(player, SkillType.Medicine) : 0;
            int recommendedLevel = item.RecommendedLevel;
            float duration = 30.0f;
            int perkLevel = player.IsPlayer ? PerkService.GetCreaturePerkLevel(player, PerkType.StimFiend) : 0;
            float percentIncrease = perkLevel * 0.25f;
            duration = duration + (duration * percentIncrease);
            Effect effect = NWScript.EffectAbilityIncrease(ability, amount);
            effect = NWScript.TagEffect(effect, "STIM_PACK_EFFECT");

            NWScript.ApplyEffectToObject(DurationType.Temporary, effect, target, duration);

            user.SendMessage("You inject " + target.Name + " with a stim pack. The stim pack will expire in " + duration + " seconds.");

            NWScript.DelayCommand(duration + 0.5f, () => { player.SendMessage("The stim pack that you applied to " + target.Name + " has expired."); });

            if (!Equals(user, target))
            {
                NWCreature targetCreature = target.Object;
                targetCreature.SendMessage(user.Name + " injects you with a stim pack.");
            }

            int xp = (int)SkillService.CalculateRegisteredSkillLevelAdjustedXP(300, item.RecommendedLevel, rank);
            SkillService.GiveSkillXP(player, SkillType.Medicine, xp);
        }

        public float Seconds(NWCreature user, NWItem item, NWObject target, Location targetLocation, CustomData customData)
        {
            return 3.0f;
        }

        public bool FaceTarget()
        {
            return true;
        }

        public int AnimationID()
        {
            return NWScript.ANIMATION_LOOPING_GET_MID;
        }

        public float MaxDistance(NWCreature user, NWItem item, NWObject target, Location targetLocation)
        {
            return 3.5f + PerkService.GetCreaturePerkLevel(user.Object, PerkType.RangedHealing);
        }

        public bool ReducesItemCharge(NWCreature user, NWItem item, NWObject target, Location targetLocation, CustomData customData)
        {
            return true;
        }

        public string IsValidTarget(NWCreature user, NWItem item, NWObject target, Location targetLocation)
        {
            var existing = target.Effects.SingleOrDefault(x => NWScript.GetEffectTag(x) == "STIM_PACK_EFFECT");

            if (existing != null && NWScript.GetIsEffectValid(existing) == NWScript.TRUE)
            {
                return "Your target is already under the effects of another stimulant.";
            }

            return null;
        }

        public bool AllowLocationTarget()
        {
            return false;
        }
    }
}
