using System;
using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.StructureStorage
{
    public class OnUsed: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlayer oPC = (NWScript.GetLastUsedBy());
            NWPlaceable container = (NWScript.OBJECT_SELF);
            Guid structureID = new Guid(container.GetLocalString("PC_BASE_STRUCTURE_ID"));
            
            if (!BasePermissionService.HasStructurePermission(oPC, structureID, StructurePermission.CanAccessStructureInventory))
            {
                oPC.FloatingText("You do not have permission to access this structure.");
                return;
            }
            
            DialogService.StartConversation(oPC, container, "StructureStorage");
        }
    }
}
