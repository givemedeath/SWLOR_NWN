﻿using SWLOR.Game.Server.Core.NWNX;
using SWLOR.Game.Server.Core.NWScript;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Core.NWScript.Enum.Item;
using SWLOR.Game.Server.Core.NWScript.Enum.VisualEffect;
using SWLOR.Game.Server.Legacy.Event.SWLOR;
using SWLOR.Game.Server.Legacy.GameObject;
using SWLOR.Game.Server.Legacy.Service;
using SWLOR.Game.Server.Legacy.ValueObject.Dialog;
using SWLOR.Game.Server.Service;
using SkillType = SWLOR.Game.Server.Legacy.Enumeration.SkillType;

namespace SWLOR.Game.Server.Legacy.Conversation
{
    public class MolecularReassembly: ConversationBase
    {
        public override PlayerDialog SetUp(NWPlayer player)
        {
            var dialog = new PlayerDialog("MainPage");
            var mainPage = new DialogPage(); // Dynamically built
            var salvagePage = new DialogPage("<SET LATER>",
                "Reassemble Component(s)"); 
            
            dialog.AddPage("MainPage", mainPage);
            dialog.AddPage("SalvagePage", salvagePage);
            return dialog;
        }

        public override void Initialize()
        {
            LoadMainPage();
        }

        public override void DoAction(NWPlayer player, string pageName, int responseID)
        {
            switch (pageName)
            {
                case "MainPage":
                    MainPageResponses(responseID);
                    break;
                case "SalvagePage":
                    SalvagePageResponses(responseID);
                    break;
            }
        }

        private void LoadMainPage()
        {
            var header = ColorToken.Green("Molecular Reassembler") + "\n\n";
            header += "This device can be used to salvage equipment and reassemble them into components.\n\n";
            header += "Please select the type of item you wish to create. The new item(s) created will have a chance to receive property bonuses from the salvaged item.\n";
            header += "A 'Reassembly Power Unit' must be in your inventory in order to reassemble an item. This will be consumed when you start the process.\n\n";
            header += "Start by selecting a component type now.";
            SetPageHeader("MainPage", header);

            ClearPageResponses("MainPage");
            var componentTypes = DataService.ComponentType.GetAllWhereHasReassembledResref();
            foreach (var type in componentTypes)
            {
                AddResponseToPage("MainPage", type.Name, true, type.ID);
            }
        }

        private void MainPageResponses(int responseID)
        {
            var player = GetPC();
            var model = CraftService.GetPlayerCraftingData(player);
            var response = GetResponseByID("MainPage", responseID);
            model.SalvageComponentTypeID = (int)response.CustomData;

            LoadSalvagePage();
            ChangePage("SalvagePage");
        }
        
        private void LoadSalvagePage()
        {
            var player = GetPC();
            var model = CraftService.GetPlayerCraftingData(player);
            NWPlaceable tempStorage = NWScript.GetObjectByTag("TEMP_ITEM_STORAGE");
            var item = SerializationService.DeserializeItem(model.SerializedSalvageItem, tempStorage);
            var componentType = DataService.ComponentType.GetByID(model.SalvageComponentTypeID);
            var header = ColorToken.Green("Item: ") + item.Name + "\n\n";
            header += "Reassembling this item will create the following " + ColorToken.Green(componentType.Name) + " component(s). Chance to create depends on your perks, skills, and harvesting bonus on items.\n\n";

            // Always create one item with zero bonuses.
            header += componentType.Name + " (No Bonuses) [RL: 0] " + GetChanceColor(100) + "\n";

            // Start by checking attack bonus since we're not storing this value as a local variable on the item.
            foreach (var prop in item.ItemProperties)
            {
                var propTypeID = NWScript.GetItemPropertyType(prop);
                if (propTypeID == ItemPropertyType.AttackBonus)
                {
                    // Get the amount of Attack Bonus
                    var amount = NWScript.GetItemPropertyCostTableValue(prop);
                    header += ProcessPropertyDetails(amount, componentType.Name, "Attack Bonus", 3);
                }
            }

            // Now check specific custom properties which are stored as local variables on the item.
            header += ProcessPropertyDetails(item.HarvestingBonus, componentType.Name, "Harvesting Bonus", 3);
            header += ProcessPropertyDetails(item.PilotingBonus, componentType.Name, "Piloting Bonus", 3);
            header += ProcessPropertyDetails(item.ScanningBonus, componentType.Name, "Scanning Bonus", 3);
            header += ProcessPropertyDetails(item.ScavengingBonus, componentType.Name, "Scavenging Bonus", 3);
            header += ProcessPropertyDetails(item.CooldownRecovery, componentType.Name, "Cooldown Recovery", 3);
            header += ProcessPropertyDetails(item.CraftBonusArmorsmith, componentType.Name, "Armorsmith", 3);
            header += ProcessPropertyDetails(item.CraftBonusWeaponsmith, componentType.Name, "Weaponsmith", 3);
            header += ProcessPropertyDetails(item.CraftBonusCooking, componentType.Name, "Cooking", 3);
            header += ProcessPropertyDetails(item.CraftBonusEngineering, componentType.Name, "Engineering", 3);
            header += ProcessPropertyDetails(item.CraftBonusFabrication, componentType.Name, "Fabrication", 3);
            header += ProcessPropertyDetails(item.HPBonus, componentType.Name, "HP", 5, 0.5f);
            header += ProcessPropertyDetails(item.FPBonus, componentType.Name, "FP", 5, 0.5f);
            header += ProcessPropertyDetails(item.EnmityRate, componentType.Name, "Enmity", 3);
            header += ProcessPropertyDetails(item.LuckBonus, componentType.Name, "Luck", 3);
            header += ProcessPropertyDetails(item.MeditateBonus, componentType.Name, "Meditate", 3);
            header += ProcessPropertyDetails(item.RestBonus, componentType.Name, "Rest", 3);
            header += ProcessPropertyDetails(item.MedicineBonus, componentType.Name, "Medicine", 3);
            header += ProcessPropertyDetails(item.HPRegenBonus, componentType.Name, "HP Regen", 3);
            header += ProcessPropertyDetails(item.FPRegenBonus, componentType.Name, "FP Regen", 3);
            header += ProcessPropertyDetails(item.StructureBonus, componentType.Name, "Structure Bonus", 3);
            header += ProcessPropertyDetails(item.SneakAttackBonus, componentType.Name, "Sneak Attack", 3);
            header += ProcessPropertyDetails(item.DamageBonus, componentType.Name, "Damage", 3);
            header += ProcessPropertyDetails(item.StrengthBonus, componentType.Name, "STR", 3);
            header += ProcessPropertyDetails(item.DexterityBonus, componentType.Name, "DEX", 3);
            header += ProcessPropertyDetails(item.ConstitutionBonus, componentType.Name, "CON", 3);
            header += ProcessPropertyDetails(item.WisdomBonus, componentType.Name, "WIS", 3);
            header += ProcessPropertyDetails(item.IntelligenceBonus, componentType.Name, "INT", 3);
            header += ProcessPropertyDetails(item.CharismaBonus, componentType.Name, "CHA", 3);
            header += ProcessPropertyDetails(item.DurationBonus, componentType.Name, "Duration", 3);


            SetPageHeader("SalvagePage", header);

            // Remove the temporary copy from the game world.
            item.Destroy();
        }

        private string GetChanceColor(int chance)
        {
            var message = "-" + chance + "%-";
            if (chance <= 50)
                return ColorToken.Red(message);
            else if (chance <= 80)
                return ColorToken.Yellow(message);
            else return ColorToken.Green(message);
        }

        private string ProcessPropertyDetails(int amount, string componentName, string propertyName, int maxBonuses, float levelsPerBonus = 1.0f)
        {
            var player = GetPC();
            var result = string.Empty;
            var penalty = 0;
            while (amount > 0)
            {
                if (amount >= maxBonuses)
                {
                    var levelIncrease = (int)(maxBonuses * levelsPerBonus);
                    var chanceToTransfer = CraftService.CalculateReassemblyChance(player, penalty);
                    result += componentName + " (+" + maxBonuses + " " + propertyName + ") [RL: " + levelIncrease + "] " + GetChanceColor(chanceToTransfer) + "\n";
                    penalty += (maxBonuses * 5);
                    amount -= maxBonuses;
                }
                else
                {
                    var levelIncrease = (int)(amount * levelsPerBonus);
                    var chanceToTransfer = CraftService.CalculateReassemblyChance(player, penalty);
                    result += componentName + " (+" + amount + " " + propertyName + ") [RL: " + levelIncrease + "] " + GetChanceColor(chanceToTransfer)+ "\n";
                    break;
                }
            }

            return result;
        }

        private void SalvagePageResponses(int responseID)
        {
            var player = GetPC();
            var model = CraftService.GetPlayerCraftingData(player);

            switch (responseID)
            {
                case 1: // Reassemble Component(s)

                    NWItem fuel = NWScript.GetItemPossessedBy(player, "ass_power");
                    // Look for reassembly fuel in the player's inventory.
                    if (!fuel.IsValid)
                    {
                        player.SendMessage(ColorToken.Red("You must have a 'Reassembly Fuel Cell' in your inventory in order to start this process."));
                        return;
                    }

                    if (model.IsConfirmingReassemble)
                    {
                        // Calculate delay, fire off delayed event, and show timing bar.
                        var delay = CraftService.CalculateCraftingDelay(player, (int) SkillType.Harvesting);
                        Player.StartGuiTimingBar(player, delay, string.Empty);
                        var @event = new OnReassembleComplete(player, model.SerializedSalvageItem, model.SalvageComponentTypeID);
                        player.DelayEvent(delay, @event);

                        // Make the player play an animation.
                        player.AssignCommand(() =>
                        {
                            NWScript.ClearAllActions();
                            NWScript.ActionPlayAnimation(Animation.LoopingGetMid, 1.0f, delay);
                        });

                        // Show sparks halfway through the process.
                        NWScript.DelayCommand(1.0f * (delay / 2.0f), () =>
                        {
                            NWScript.ApplyEffectToObject(DurationType.Instant, NWScript.EffectVisualEffect(VisualEffect.Vfx_Com_Blood_Spark_Medium), NWScript.OBJECT_SELF);
                        });
                        
                        // Immobilize the player while crafting.
                        var immobilize = NWScript.EffectCutsceneImmobilize();
                        immobilize = NWScript.TagEffect(immobilize, "CRAFTING_IMMOBILIZATION");
                        NWScript.ApplyEffectToObject(DurationType.Permanent, immobilize, player);

                        // Clear the temporary crafting data and end this conversation.
                        model.SerializedSalvageItem = string.Empty;
                        EndConversation();
                    }
                    else
                    {
                        model.IsConfirmingReassemble = true;
                        SetResponseText("SalvagePage", 1, "CONFIRM REASSEMBLE COMPONENT(S)");
                    }
                    break;
            }
        }

        public override void Back(NWPlayer player, string beforeMovePage, string afterMovePage)
        {
            var model = CraftService.GetPlayerCraftingData(player);
            model.IsConfirmingReassemble = false;
            SetResponseText("SalvagePage", 1, "Reassemble Component(s)");
        }

        public override void EndDialog()
        {
            var player = GetPC();
            CraftService.ClearPlayerCraftingData(player);
        }
    }
}