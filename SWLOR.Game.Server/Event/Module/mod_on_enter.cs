﻿using SWLOR.Game.Server;
using SWLOR.Game.Server.Event.Module;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Messaging;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.ValueObject;


// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class mod_on_enter
#pragma warning restore IDE1006 // Naming Styles
    {
        // ReSharper disable once UnusedMember.Local
        public static void Main()
        {
            // The order of the following procedures matters.
            NWPlayer player = SWLOR.Game.Server.NWN.NWScript.GetEnteringObject();

            using (new Profiler(nameof(mod_on_enter) + ":AddDMToCache"))
            {
                if (player.IsDM)
                {
                    AppCache.ConnectedDMs.Add(player);
                }
            }

            using (new Profiler(nameof(mod_on_enter) + ":BiowareDefault"))
            {
                player.DeleteLocalInt("IS_CUSTOMIZING_ITEM");
                SWLOR.Game.Server.NWN.NWScript.ExecuteScript("dmfi_onclienter ", SWLOR.Game.Server.NWN.NWScript.OBJECT_SELF); // DMFI also calls "x3_mod_def_enter"
            }

            using (new Profiler(nameof(mod_on_enter) + ":PlayerValidation"))
            {
                PlayerValidationService.OnModuleEnter();
            }

            using (new Profiler(nameof(mod_on_enter) + ":InitializePlayer"))
            {
                PlayerService.InitializePlayer(player);
            }

            using (new Profiler(nameof(mod_on_enter) + ":SkillServiceEnter"))
            {
                SkillService.OnModuleEnter();
            }

            using (new Profiler(nameof(mod_on_enter) + ":PerkServiceEnter"))
            {
                PerkService.OnModuleEnter();
            }
            
            MessageHub.Instance.Publish(new OnModuleEnter());
            player.SetLocalInt("LOGGED_IN_ONCE", SWLOR.Game.Server.NWN.NWScript.TRUE);
        }
    }
}
