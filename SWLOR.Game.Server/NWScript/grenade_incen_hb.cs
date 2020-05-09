using NWN;
using System;
using static SWLOR.Game.Server.NWN.NWScript;
using SWLOR.Game.Server.GameObject;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class grenade_incen_hb
#pragma warning restore IDE1006 // Naming Styles
    {
        // ReSharper disable once UnusedMember.Local
        private static void Main()
        {            
            NWObject oTarget;            
            oTarget = GetFirstInPersistentObject(SWLOR.Game.Server.NWN.NWScript.OBJECT_SELF);
            while (GetIsObjectValid(oTarget) == TRUE)
            {         
                SWLOR.Game.Server.Item.Grenade.grenadeAoe(oTarget, "INCENDIARY");
                //Get the next target in the AOE
                oTarget = GetNextInPersistentObject(SWLOR.Game.Server.NWN.NWScript.OBJECT_SELF);
            }
        }
    }
}