﻿using static SWLOR.Game.Server.Core.NWScript.NWScript;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class grenade_smoke_en
#pragma warning restore IDE1006 // Naming Styles
    {
        // ReSharper disable once UnusedMember.Local
        public static void Main()
        {
            //MessageHub.Instance.Publish(new OnItemUsed());
            SWLOR.Game.Server.Item.Grenade.grenadeAoe(GetEnteringObject(), "SMOKE");
        }
    }
}