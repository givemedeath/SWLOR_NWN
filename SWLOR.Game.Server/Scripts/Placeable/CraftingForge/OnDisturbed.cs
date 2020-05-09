﻿using System.Linq;
using NWN;
using SWLOR.Game.Server.Bioware;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Event.SWLOR;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.NWNX;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.CraftingForge
{
    public class OnDisturbed: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            if (NWScript.GetInventoryDisturbType() != NWScript.INVENTORY_DISTURB_TYPE_ADDED) return;

            NWPlayer pc = (NWScript.GetLastDisturbed());
            NWItem item = (NWScript.GetInventoryDisturbItem());
            NWPlaceable forge = (NWScript.OBJECT_SELF);

            if (!CheckValidity(forge, pc, item)) return;
            StartSmelt(forge, pc, item);
        }


        private bool CheckValidity(NWPlaceable forge, NWPlayer pc, NWItem item)
        {
            if (pc.IsBusy)
            {
                ReturnItemToPC(pc, item, "You are too busy.");
                return false;
            }

            if (NWScript.GetIsObjectValid(forge.GetLocalObject("FORGE_USER")) == NWScript.TRUE)
            {
                ReturnItemToPC(pc, item, "This forge is currently in use. Please wait...");
                return false;
            }

            string[] allowed = {
                "power_core",
                "raw_veldite",
                "raw_scordspar",
                "raw_plagionite",
                "raw_keromber",
                "raw_jasioclase",
                "raw_hemorgite",
                "raw_ochne",
                "raw_croknor",
                "raw_arkoxit",
                "raw_bisteiss"
        };

            if (!allowed.Contains(item.Resref))
            {
                ReturnItemToPC(pc, item, "Only power cores and raw materials may be placed inside.");
                return false;
            }

            int level = CraftService.GetIngotLevel(item.Resref);
            int rank = SkillService.GetPCSkillRank(pc, SkillType.Harvesting);
            
            int delta = rank - level;
            if (delta <= -4)
            {
                ReturnItemToPC(pc, item, "You do not have enough skill to refine this material.");
                return false;
            }

            int pcPerkLevel = PerkService.GetCreaturePerkLevel(pc, PerkType.Refining);
            int orePerkLevel = CraftService.GetIngotPerkLevel(item.Resref);

            if (pcPerkLevel < orePerkLevel)
            {
                ReturnItemToPC(pc, item, "You do not have the perk necessary to refine this material.");
                return false;
            }

            return true;
        }

        private void StartSmelt(NWPlaceable forge, NWPlayer pc, NWItem item)
        {
            int charges = forge.GetLocalInt("FORGE_CHARGES");
            if (item.Resref == "power_core")
            {
                item.Destroy();
                charges += 10 + CalculatePerkCoalBonusCharges(pc) + GetPowerCoreDurability(item) * 2;
                forge.SetLocalInt("FORGE_CHARGES", charges);

                NWPlaceable flames = (forge.GetLocalObject("FORGE_FLAMES"));
                if (!flames.IsValid)
                {
                    Vector flamePosition = BiowarePosition.GetChangedPosition(forge.Position, 0.36f, forge.Facing);
                    Location flameLocation = NWScript.Location(forge.Area.Object, flamePosition, 0.0f);
                    flames = (NWScript.CreateObject(ObjectType.Placeable, "forge_flame", flameLocation));
                    forge.SetLocalObject("FORGE_FLAMES", flames.Object);
                }

                return;
            }
            else if (charges <= 0)
            {
                ReturnItemToPC(pc, item, "You must power the refinery with a power unit before refining.");
                return;
            }

            // Ready to smelt
            float baseCraftDelay = 18.0f - (18.0f * PerkService.GetCreaturePerkLevel(pc, PerkType.SpeedyRefining) * 0.1f);

            pc.IsBusy = true;
            NWNXPlayer.StartGuiTimingBar(pc, baseCraftDelay, string.Empty);

            // Any component bonuses on the ore get applied to the end product.
            var itemProperties = item.ItemProperties.Where(x =>
                NWScript.GetItemPropertyType(x) == (int)ItemPropertyType.ComponentBonus ||
                NWScript.GetItemPropertyType(x) == (int) ItemPropertyType.RecommendedLevel).ToList();

            string itemResref = item.Resref;

            var @event = new OnCompleteSmelt(pc, itemResref, itemProperties);
            pc.DelayEvent(baseCraftDelay, @event);
            
            NWScript.ApplyEffectToObject(DurationType.Temporary, NWScript.EffectCutsceneImmobilize(), pc.Object, baseCraftDelay);
            pc.AssignCommand(() => NWScript.ActionPlayAnimation(NWScript.ANIMATION_LOOPING_GET_MID, 1.0f, baseCraftDelay));
            item.Destroy();
        }

        private int GetPowerCoreDurability(NWItem item)
        {
            int durability = 0;
            foreach (var ip in item.ItemProperties)
            {
                if (NWScript.GetItemPropertyType(ip) == (int)ItemPropertyType.ComponentBonus)
                {
                    int bonusTypeID = NWScript.GetItemPropertySubType(ip);
                    if (bonusTypeID == (int) ComponentBonusType.DurabilityUp)
                    {
                        int amount = NWScript.GetItemPropertyCostTableValue(ip);
                        durability += amount;
                    }
                }
            }

            return durability;
        }

        private void ReturnItemToPC(NWPlayer pc, NWItem item, string message)
        {
            NWScript.CopyItem(item.Object, pc.Object, NWScript.TRUE);
            item.Destroy();
            pc.SendMessage(message);
        }

        private int CalculatePerkCoalBonusCharges(NWPlayer pc)
        {
            int perkLevel = PerkService.GetCreaturePerkLevel(pc, PerkType.RefineryManagement);

            switch (perkLevel)
            {
                case 1: return 2;
                case 2: return 3;
                case 3: return 4;
                case 4: return 5;
                case 5: return 8;
                case 6: return 10;
                default: return 0;
            }
        }
    }
}
