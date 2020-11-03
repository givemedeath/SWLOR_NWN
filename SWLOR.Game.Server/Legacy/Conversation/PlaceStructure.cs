﻿using System;
using System.Linq;
using System.Numerics;
using SWLOR.Game.Server.Core.NWNX;
using SWLOR.Game.Server.Core.NWNX.Enum;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Core.NWScript.Enum.Area;
using SWLOR.Game.Server.Core.NWScript.Enum.VisualEffect;
using SWLOR.Game.Server.Legacy.Data.Entity;
using SWLOR.Game.Server.Legacy.Enumeration;
using SWLOR.Game.Server.Legacy.GameObject;
using SWLOR.Game.Server.Legacy.Service;
using SWLOR.Game.Server.Legacy.ValueObject.Dialog;
using SWLOR.Game.Server.Service;
using static SWLOR.Game.Server.Core.NWScript.NWScript;
using BaseStructureType = SWLOR.Game.Server.Legacy.Enumeration.BaseStructureType;
using BuildingType = SWLOR.Game.Server.Legacy.Enumeration.BuildingType;

namespace SWLOR.Game.Server.Legacy.Conversation
{
    public class PlaceStructure : ConversationBase
    {
        public override PlayerDialog SetUp(NWPlayer player)
        {
            var dialog = new PlayerDialog("MainPage");
            var mainPage = new DialogPage(string.Empty,
                "Place Structure",
                "Rotate/Move",
                "Preview",
                "Change Exterior Style",
                "Change Interior Style");

            var rotatePage = new DialogPage(string.Empty,
                "East",
                "North",
                "West",
                "South",
                "20 degrees",
                "30 degrees",
                "45 degrees",
                "60 degrees",
                "75 degrees",
                "90 degrees",
                "180 degrees",
                "Move up",
                "Move down");

            var stylePage = new DialogPage();

            dialog.AddPage("MainPage", mainPage);
            dialog.AddPage("RotatePage", rotatePage);
            dialog.AddPage("StylePage", stylePage);

            // Setup placement grid                
            var area = GetArea(player);
            var width = GetAreaSize(Dimension.Width, area);
            var height = GetAreaSize(Dimension.Height, area);
            Vector3 vPos;
            vPos.X = 5.0f;
            vPos.Y = 0.0f;
            vPos.Z = 0.1f;
            for (var i = 0; i <= height; i++)
            {
                vPos.Y = -5.0f;
                for (var j = 0; j <= width; j++)
                {
                    vPos.Y += 10.0f;
                    NWObject oTile = CreateObject(ObjectType.Placeable, "plc_invisobj",
                                                        Location(area, vPos, 0.0f), false,
                                                        "x2_tmp_tile" + player.GlobalID.ToString());
                    SetPlotFlag(oTile, true);
                    ApplyEffectToObject(DurationType.Permanent, EffectVisualEffect(VisualEffect.Vfx_Placement_Grid), oTile);
                    Visibility.SetVisibilityOverride(OBJECT_INVALID, oTile, VisibilityType.Hidden);
                    Visibility.SetVisibilityOverride(player, oTile, VisibilityType.Visible);                    
                    mainPage.CustomData.Add("PLACEMENT_GRID_" + i + "_" + j, oTile);
                }
                vPos.X += 10.0f;
            }

            return dialog;
        }

        public override void Initialize()
        {
            LoadMainPage();
        }

        private void LoadMainPage()
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            var structure = DataService.BaseStructure.GetByID(data.BaseStructureID);
            var tower = BaseService.GetBaseControlTower(data.PCBaseID);
            var towerBaseStructure = tower == null ? null : DataService.BaseStructure.GetByID(tower.BaseStructureID);

            var canPlaceStructure = true;
            var isPlacingTower = structure.BaseStructureTypeID == (int)BaseStructureType.ControlTower;
            var isPlacingBuilding = structure.BaseStructureTypeID == (int)BaseStructureType.Building;
            var canChangeBuildingStyles = isPlacingBuilding && data.StructureItem.GetLocalBool("STRUCTURE_BUILDING_INITIALIZED") == false;

            var powerInUse = BaseService.GetPowerInUse(data.PCBaseID);
            var cpuInUse = BaseService.GetCPUInUse(data.PCBaseID);

            var towerPower = tower != null ? towerBaseStructure.Power + (tower.StructureBonus * 3) : 0.0f;
            var towerCPU = tower != null ? towerBaseStructure.CPU + (tower.StructureBonus * 2) : 0.0f;
            var newPower = powerInUse + structure.Power;
            var newCPU = cpuInUse + structure.CPU;

            var insufficientPower = newPower > towerPower && !isPlacingTower ? ColorToken.Red(" (INSUFFICIENT POWER)") : string.Empty;
            var insufficientCPU = newCPU > towerCPU && !isPlacingTower ? ColorToken.Red(" (INSUFFICIENT CPU)") : string.Empty;

            var header = ColorToken.Green("Structure: ") + structure.Name + "\n";

            if (data.BuildingType == BuildingType.Interior)
            {
                var buildingStructure = DataService.PCBaseStructure.GetByID((Guid)data.ParentStructureID);
                var baseStructure = DataService.BaseStructure.GetByID(buildingStructure.BaseStructureID);
                var childStructures = DataService.PCBaseStructure.GetAllByParentPCBaseStructureID(buildingStructure.ID).ToList();

                header += ColorToken.Green("Structure Limit: ") + childStructures.Count + " / " + (baseStructure.Storage + buildingStructure.StructureBonus) + "\n";
                var structures = DataService.PCBaseStructure
                    .GetAllByParentPCBaseStructureID(buildingStructure.ID).Where(x =>
                    {
                        var childStructure = DataService.BaseStructure.GetByID(x.BaseStructureID);
                        return childStructure.HasAtmosphere;
                    });

                // Add up the total atmosphere rating, being careful not to go over the cap.
                var bonus = structures.Sum(x => 1 + x.StructureBonus) * 2;
                if (bonus > 150) bonus = 150;
                header += ColorToken.Green("Atmosphere Bonus: ") + bonus + "% / " + "150%";
                header += "\n";
            }
            else if (data.BuildingType == BuildingType.Starship)
            {
                var buildingStructure = DataService.PCBaseStructure.GetByID((Guid)data.ParentStructureID);
                var buildingStyle = DataService.BuildingStyle.GetByID(Convert.ToInt32(buildingStructure.InteriorStyleID));
                var childStructures = DataService.PCBaseStructure.GetAllByParentPCBaseStructureID(buildingStructure.ID).ToList();

                header += ColorToken.Green("Structure Limit: ") + childStructures.Count + " / " + (buildingStyle.FurnitureLimit + buildingStructure.StructureBonus) + "\n";
                var structures = DataService.PCBaseStructure
                    .GetAllByParentPCBaseStructureID(buildingStructure.ID).Where(x =>
                    {
                        var childStructure = DataService.BaseStructure.GetByID(x.BaseStructureID);
                        return childStructure.HasAtmosphere;
                    });

                // Add up the total atmosphere rating, being careful not to go over the cap.
                var bonus = structures.Sum(x => 1 + x.StructureBonus) * 2;
                if (bonus > 150) bonus = 150;
                header += ColorToken.Green("Atmosphere Bonus: ") + bonus + "% / " + "150%";
                header += "\n";
            }
            else if (data.BuildingType == BuildingType.Apartment)
            {
                var pcBase = DataService.PCBase.GetByID(data.PCBaseID);
                var buildingStyle = DataService.BuildingStyle.GetByID(Convert.ToInt32(pcBase.BuildingStyleID));
                var structures = DataService.PCBaseStructure.GetAllByPCBaseID(pcBase.ID).ToList();
                header += ColorToken.Green("Structure Limit: ") + structures.Count + " / " + buildingStyle.FurnitureLimit + "\n";
                var bonus = structures.Sum(x => 1 + x.StructureBonus) * 2;
                if (bonus > 150) bonus = 150;
                header += ColorToken.Green("Atmosphere Bonus: ") + bonus + "% / " + "150%";
                header += "\n";
            }
            else if(data.BuildingType == BuildingType.Exterior)
            {
                if (isPlacingTower)
                {
                    header += ColorToken.Green("Available Power: ") + (structure.Power + data.StructureItem.StructureBonus * 3) + "\n";
                    header += ColorToken.Green("Available CPU: ") + (structure.CPU + data.StructureItem.StructureBonus * 2) + "\n";
                }
                else
                {
                    header += ColorToken.Green("Base Power: ") + powerInUse + " / " + towerPower + "\n";
                    header += ColorToken.Green("Base CPU: ") + cpuInUse + " / " + towerCPU + "\n";
                    header += ColorToken.Green("Required Power: ") + structure.Power + insufficientPower + "\n";
                    header += ColorToken.Green("Required CPU: ") + structure.CPU + insufficientCPU + "\n";
                }
            }

            if (isPlacingBuilding)
            {
                var exteriorStyle = data.StructureItem.GetLocalInt("STRUCTURE_BUILDING_EXTERIOR_ID");
                var interiorStyle = data.StructureItem.GetLocalInt("STRUCTURE_BUILDING_INTERIOR_ID");
                var exterior = DataService.BuildingStyle.GetByID(exteriorStyle);
                var interior = DataService.BuildingStyle.GetByID(interiorStyle);

                header += ColorToken.Green("Exterior Style: ") + exterior.Name + "\n";
                header += ColorToken.Green("Interior Style: ") + interior.Name + "\n";
            }

            if (!isPlacingTower && (newPower > towerPower || newCPU > towerCPU))
            {
                canPlaceStructure = false;
                header += "\nOne or more requirements not met. Cannot place structure.";
            }

            SetPageHeader("MainPage", header);
            SetResponseVisible("MainPage", 1, canPlaceStructure);
            SetResponseVisible("MainPage", 2, canPlaceStructure);
            SetResponseVisible("MainPage", 3, canPlaceStructure);
            SetResponseVisible("MainPage", 4, canPlaceStructure && canChangeBuildingStyles);
            SetResponseVisible("MainPage", 5, canPlaceStructure && canChangeBuildingStyles);
        }

        public override void DoAction(NWPlayer player, string pageName, int responseID)
        {
            switch (pageName)
            {
                case "MainPage":
                    MainResponses(responseID);
                    break;
                case "RotatePage":
                    RotateResponses(responseID);
                    break;
                case "StylePage":
                    StyleResponses(responseID);
                    break;
            }
        }

        public override void Back(NWPlayer player, string beforeMovePage, string afterMovePage)
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            switch (beforeMovePage)
            {
                case "RotatePage":
                    if (data.StructurePreview != null && data.StructurePreview.IsValid)
                    {
                        data.StructurePreview.Destroy();
                    }
                    break;
            }
        }

        private void MainResponses(int responseID)
        {
            switch (responseID)
            {
                case 1: // Place Structure
                    DoPlaceStructure();
                    break;
                case 2: // Rotate
                    LoadRotatePage();
                    ChangePage("RotatePage");
                    break;
                case 3: // Preview
                    Preview();
                    break;
                case 4: // Change Exterior Style
                    LoadStylePage(BuildingType.Exterior);
                    ChangePage("StylePage");
                    break;
                case 5: // Change Interior Style
                    LoadStylePage(BuildingType.Interior);
                    ChangePage("StylePage");
                    break;
            }
        }

        private string GetPlaceableResref(BaseStructure structure)
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            var resref = structure.PlaceableResref;

            if (string.IsNullOrWhiteSpace(resref) &&
                structure.BaseStructureTypeID == (int)BaseStructureType.Building)
            {
                var exteriorID = data.StructureItem.GetLocalInt("STRUCTURE_BUILDING_EXTERIOR_ID");
                var style = DataService.BuildingStyle.GetByID(exteriorID);

                resref = style.Resref;
            }

            return resref;
        }

        private void Preview()
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            if (data.IsPreviewing) return;

            data.IsPreviewing = true;
            var structure = DataService.BaseStructure.GetByID(data.BaseStructureID);
            var resref = GetPlaceableResref(structure);

            NWPlaceable plc = (CreateObject(ObjectType.Placeable, resref, data.TargetLocation));
            plc.IsUseable = false;
            plc.Destroy(6.0f);
            DelayCommand(6.1f, () => { data.IsPreviewing = false; });
            ApplyEffectToObject(DurationType.Permanent, EffectVisualEffect(VisualEffect.Vfx_Dur_Aura_Green), plc.Object);
        }

        private void LoadRotatePage()
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            var facing = GetFacingFromLocation(data.TargetLocation);
            var position = GetPositionFromLocation(data.TargetLocation);
            var header = ColorToken.Green("Current Direction: ") + facing + "\n\n";
            header += ColorToken.Green("Current Height: ") + position.Z;

            if (data.StructurePreview == null || !data.StructurePreview.IsValid)
            {
                var structure = DataService.BaseStructure.GetByID(data.BaseStructureID);
                var resref = GetPlaceableResref(structure);
                data.StructurePreview = (CreateObject(ObjectType.Placeable, resref, data.TargetLocation));
                data.StructurePreview.IsUseable = false;
                ApplyEffectToObject(DurationType.Permanent, EffectVisualEffect(VisualEffect.Vfx_Dur_Aura_Green), data.StructurePreview.Object);
            }

            SetPageHeader("RotatePage", header);
        }

        private void RotateResponses(int responseID)
        {
            switch (responseID)
            {
                case 1: // East
                    DoRotate(0.0f, true);
                    break;
                case 2: // North
                    DoRotate(90.0f, true);
                    break;
                case 3: // West
                    DoRotate(180.0f, true);
                    break;
                case 4: // South
                    DoRotate(270.0f, true);
                    break;
                case 5: // Rotate 20
                    DoRotate(20.0f, false);
                    break;
                case 6: // Rotate 30
                    DoRotate(30.0f, false);
                    break;
                case 7: // Rotate 45
                    DoRotate(45.0f, false);
                    break;
                case 8: // Rotate 60
                    DoRotate(60.0f, false);
                    break;
                case 9: // Rotate 75
                    DoRotate(75.0f, false);
                    break;
                case 10: // Rotate 90
                    DoRotate(90.0f, false);
                    break;
                case 11: // Rotate 180
                    DoRotate(180.0f, false);
                    break;
                case 12: // Move Up
                    DoMoveZ(0.1f, false);
                    break;
                case 13: // Move Down
                    DoMoveZ(-0.1f, false);
                    break;
            }
        }

        private void DoRotate(float degrees, bool isSet)
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            var facing = GetFacingFromLocation(data.TargetLocation);
            if (isSet)
            {
                facing = degrees;
            }
            else
            {
                facing += degrees;
            }

            while (facing > 360)
            {
                facing -= 360;
            }

            if (data.StructurePreview != null && data.StructurePreview.IsValid)
            {
                data.StructurePreview.Facing = facing;
            }

            data.TargetLocation = Location(GetAreaFromLocation(data.TargetLocation),
                GetPositionFromLocation(data.TargetLocation),
                facing);
            LoadRotatePage();
        }

        private void DoMoveZ(float degrees, bool isSet)
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            var position = GetPositionFromLocation(data.TargetLocation);
            
            if (position.Z > 10.0f || 
                position.Z < -10.0f)
            {
                GetPC().SendMessage("This structure cannot be moved any further in this direction.");                
            }
            else
            {
                position.Z += degrees;
            }

            Preview();

            data.TargetLocation = Location(GetAreaFromLocation(data.TargetLocation),
                position,
                GetFacingFromLocation(data.TargetLocation));
            LoadRotatePage();
        }

        private void DoPlaceStructure()
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            var canPlaceStructure = BaseService.CanPlaceStructure(GetPC(), data.StructureItem, data.TargetLocation, data.BaseStructureID);
            var baseStructure = DataService.BaseStructure.GetByID(data.BaseStructureID);

            if (!string.IsNullOrWhiteSpace(canPlaceStructure))
            {
                GetPC().SendMessage(canPlaceStructure);
                return;
            }

            var position = GetPositionFromLocation(data.TargetLocation);
            int? interiorStyleID = data.StructureItem.GetLocalInt("STRUCTURE_BUILDING_INTERIOR_ID");
            int? exteriorStyleID = data.StructureItem.GetLocalInt("STRUCTURE_BUILDING_EXTERIOR_ID");
            interiorStyleID = interiorStyleID <= 0 ? null : interiorStyleID;
            exteriorStyleID = exteriorStyleID <= 0 ? null : exteriorStyleID;

            var structure = new PCBaseStructure
            {
                BaseStructureID = data.BaseStructureID, 
                Durability = DurabilityService.GetDurability(data.StructureItem),
                LocationOrientation = GetFacingFromLocation(data.TargetLocation),
                LocationX = position.X,
                LocationY = position.Y,
                LocationZ = position.Z,
                PCBaseID = data.PCBaseID,
                InteriorStyleID = interiorStyleID,
                ExteriorStyleID = exteriorStyleID,
                CustomName = string.Empty,
                ParentPCBaseStructureID = data.ParentStructureID,
                StructureBonus = data.StructureItem.StructureBonus, 
                StructureModeID = baseStructure.DefaultStructureModeID
            };
            DataService.SubmitDataChange(structure, DatabaseActionType.Insert);
            
            // Placing a control tower. Set base shields to 100%
            if (baseStructure.BaseStructureTypeID == (int)BaseStructureType.ControlTower)
            {
                var pcBase = DataService.PCBase.GetByID(data.PCBaseID);
                pcBase.ShieldHP = BaseService.CalculateMaxShieldHP(structure);
                DataService.SubmitDataChange(pcBase, DatabaseActionType.Update);
            }
            
            BaseService.SpawnStructure(data.TargetArea, structure.ID);
            data.StructureItem.Destroy();
            EndConversation();
        }

        private void LoadStylePage(BuildingType buildingType)
        {
            var data = BaseService.GetPlayerTempData(GetPC());

            // Header
            var styleID = data.StructureItem.GetLocalInt("STRUCTURE_BUILDING_EXTERIOR_ID");
            if (buildingType == BuildingType.Interior)
                styleID = data.StructureItem.GetLocalInt("STRUCTURE_BUILDING_INTERIOR_ID");

            var currentStyle = DataService.BuildingStyle.GetByID(styleID);
            var header = ColorToken.Green("Building Style: ") + currentStyle.Name + "\n\n";
            header += "Change the style by selecting from the list below.";

            SetPageHeader("StylePage", header);
            // Responses
            ClearPageResponses("StylePage");

            if (buildingType == BuildingType.Interior)
            {
                AddResponseToPage("StylePage", "Preview Interior", true, new Tuple<int, BuildingType>(-2, BuildingType.Interior));
            }

            var styles = DataService.BuildingStyle.GetAll().Where(x => x.BuildingTypeID == (int)buildingType && 
                                                               x.BaseStructureID == data.BaseStructureID && x.IsActive).ToList();
            foreach (var style in styles)
            {
                var args = new Tuple<int, BuildingType>(style.ID, buildingType);
                AddResponseToPage("StylePage", style.Name, true, args);
            }
        }

        private void StyleResponses(int responseID)
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            var response = GetResponseByID("StylePage", responseID);
            var args = (Tuple<int, BuildingType>)response.CustomData;
            var styleID = args.Item1;
            var type = args.Item2;
            
            if (styleID == -2)
            {
                DoInteriorPreview();
                EndConversation();
                return;
            }

            if (type == BuildingType.Interior)
            {
                data.StructureItem.SetLocalInt("STRUCTURE_BUILDING_INTERIOR_ID", styleID);
            }
            else
            {
                data.StructureItem.SetLocalInt("STRUCTURE_BUILDING_EXTERIOR_ID", styleID);
                Preview();
            }

            LoadStylePage(type);
        }

        private void DoInteriorPreview()
        {
            var data = BaseService.GetPlayerTempData(GetPC());
            var styleID = data.StructureItem.GetLocalInt("STRUCTURE_BUILDING_INTERIOR_ID");
            var style = DataService.BuildingStyle.GetByID(styleID);
            var area = AreaService.CreateAreaInstance(GetPC(), style.Resref, "BUILDING PREVIEW: " + style.Name, "PLAYER_HOME_ENTRANCE");
            SetLocalBool(area, "IS_BUILDING_PREVIEW", true);
            var player = GetPC();

            BaseService.JumpPCToBuildingInterior(player, area);
        }

        public override void EndDialog()
        {
            // tear down placement grid
            var mainPage = GetPageByName("MainPage");
            foreach (var placementGrid in mainPage.CustomData)
            {
                if (placementGrid.Key.StartsWith("PLACEMENT_GRID_"))
                {                    
                    DelayCommand(10.0f, () => { DestroyObject(placementGrid.Value); });
                }
            }
                
            BaseService.ClearPlayerTempData(GetPC());
        }
    }
}