﻿using System;
using SWLOR.Game.Server.Core.NWScript;
using SWLOR.Game.Server.CustomEffect.Contracts;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Service;

namespace SWLOR.Game.Server.CustomEffect
{
    public class BleedingEffect: ICustomEffectHandler
    {
        public CustomEffectCategoryType CustomEffectCategoryType => CustomEffectCategoryType.NormalEffect;
        public CustomEffectType CustomEffectType => CustomEffectType.Bleeding;

        public string Apply(NWCreature oCaster, NWObject oTarget, int effectiveLevel)
        {
            return null;
        }

        public void Tick(NWCreature oCaster, NWObject oTarget, int currentTick, int effectiveLevel, string data)
        {
            if (currentTick % 2 == 0) return;

            var location = oTarget.Location;
            NWPlaceable oBlood = (NWScript.CreateObject(ObjectType.Placeable, "plc_bloodstain", location));
            oBlood.Destroy(48.0f);

            var amount = 1;

            if (!string.IsNullOrWhiteSpace(data))
            {
                amount = Convert.ToInt32(data);
            }

            oTarget.SetLocalInt(AbilityService.LAST_ATTACK + oCaster.GlobalID, AbilityService.ATTACK_DOT);

            oCaster.AssignCommand(() =>
            {
                var damage = NWScript.EffectDamage(amount);
                NWScript.ApplyEffectToObject(DurationType.Instant, damage, oTarget.Object);
            });
        }

        public void WearOff(NWCreature oCaster, NWObject oTarget, int effectiveLevel, string data)
        {
        }

        public string StartMessage => "You start bleeding.";
        public string ContinueMessage => "You continue to bleed...";
        public string WornOffMessage => "You have stopped bleeding.";
    }
}
