﻿using System;
using SWLOR.Game.Server.Core.NWScript;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Legacy.Enumeration;
using SWLOR.Game.Server.Legacy.GameObject;
using SWLOR.Game.Server.Legacy.Service;
using SWLOR.Game.Server.Legacy.ValueObject.Dialog;
using SWLOR.Game.Server.Service;

namespace SWLOR.Game.Server.Legacy.Conversation
{
    public class StructureStorage : ConversationBase
    {
        public override PlayerDialog SetUp(NWPlayer player)
        {
            var dialog = new PlayerDialog("MainPage");
            var mainPage = new DialogPage(
                ColorToken.Green("Persistent Storage Menu") + "\n\nPlease select an option.",
                "Open Storage",
                "Change Container Name"
            );

            var changeNamePage = new DialogPage(
                ColorToken.Green("Change Container Name") + "\n\nPlease type a name for the container into your chat bar and then press enter. After that's done click the 'Next' button on this conversation window.",
                "Next"
            );

            var confirmChangeName = new DialogPage(
                "<SET LATER>",
                "Confirm Name Change"
            );

            dialog.AddPage("MainPage", mainPage);
            dialog.AddPage("ChangeNamePage", changeNamePage);
            dialog.AddPage("ConfirmChangeNamePage", confirmChangeName);
            return dialog;
        }

        public override void Initialize()
        {
            var container = (NWPlaceable) GetDialogTarget();
            var structureID = new Guid(container.GetLocalString("PC_BASE_STRUCTURE_ID"));

            if (!BasePermissionService.HasStructurePermission(GetPC(), structureID, StructurePermission.CanAccessStructureInventory))
            {
                SetResponseVisible("MainPage", 1, false);
            }

            if(!BasePermissionService.HasStructurePermission(GetPC(), structureID, StructurePermission.CanRenameStructures))
            {
                SetResponseVisible("MainPage", 2, false);
            }
        }

        public override void DoAction(NWPlayer player, string pageName, int responseID)
        {
            switch (pageName)
            {
                case "MainPage":
                    HandleMainPageResponse(responseID);
                    break;
                case "ChangeNamePage":
                    HandleChangeNamePageResponse(responseID);
                    break;
                case "ConfirmChangeNamePage":
                    HandleConfirmChangeNamePageResponse(responseID);
                    break;
            }

        }

        public override void Back(NWPlayer player, string beforeMovePage, string afterMovePage)
        {
            switch (beforeMovePage)
            {
                case "ChangeNamePage":
                    ClearTempVariables();
                    break;
            }
        }

        private void HandleMainPageResponse(int responseID)
        {
            switch (responseID)
            {
                case 1: // Open Storage
                    OpenPersistentStorage();
                    EndConversation();
                    break;
                case 2: // Change Container Name
                    GetPC().SetLocalInt("LISTENING_FOR_NEW_CONTAINER_NAME", 1);
                    ChangePage("ChangeNamePage");
                    break;
            }
        }

        private void HandleChangeNamePageResponse(int responseID)
        {
            switch (responseID)
            {
                case 1: // Next
                    var name = GetPC().GetLocalString("NEW_CONTAINER_NAME");
                    if (name == "")
                    {
                        GetPC().FloatingText("Type in a new name to the chat bar and then press 'Next'.");
                        return;
                    }

                    var header = ColorToken.Green("Change Container Name") + "\n\n";
                    header += ColorToken.Green("New Container Name: ") + name + "\n\n";
                    header += "Are you sure you want to change your container to this name?";

                    SetPageHeader("ConfirmChangeNamePage", header);
                    ChangePage("ConfirmChangeNamePage");
                    break;
            }
        }

        private void HandleConfirmChangeNamePageResponse(int responseID)
        {
            switch (responseID)
            {
                case 1: // Confirm Change Name
                    var name = GetPC().GetLocalString("NEW_CONTAINER_NAME");
                    var structureID = new Guid(GetDialogTarget().GetLocalString("PC_BASE_STRUCTURE_ID"));
                    var structure = DataService.PCBaseStructure.GetByID(structureID);
                    structure.CustomName = name;
                    GetDialogTarget().Name = name;
                    GetPC().DeleteLocalString("NEW_CONTAINER_NAME");
                    DataService.SubmitDataChange(structure, DatabaseActionType.Update);
                    EndConversation();
                    break;
            }
        }

        private void ClearTempVariables()
        {
            GetPC().DeleteLocalInt("LISTENING_FOR_NEW_CONTAINER_NAME");
            GetPC().DeleteLocalString("NEW_CONTAINER_NAME");
        }

        public override void EndDialog()
        {
            ClearTempVariables();
        }

        private void OpenPersistentStorage()
        {
            var chest = (NWPlaceable)GetDialogTarget();
            var oPC = GetPC();

            if (((NWPlaceable)chest.GetLocalObject("STRUCTURE_TEMP_INVENTORY_OPENED")).IsValid)
            {
                oPC.FloatingText("Someone else is already accessing that structure's inventory. Please wait.");
                return;
            }

            var structureID = chest.GetLocalString("PC_BASE_STRUCTURE_ID");
            var location = oPC.Location;
            NWPlaceable copy = (NWScript.CreateObject(ObjectType.Placeable, "str_storage_copy", location));
            copy.Name = chest.Name;
            copy.AssignCommand(() => NWScript.SetFacingPoint(oPC.Position));

            chest.SetLocalObject("STRUCTURE_TEMP_INVENTORY_OPENED", copy.Object);
            copy.SetLocalObject("STRUCTURE_TEMP_PARENT", chest.Object);
            copy.SetLocalString("PC_BASE_STRUCTURE_ID", structureID);

            oPC.AssignCommand(() => NWScript.ActionInteractObject(copy.Object));
        }

    }
}