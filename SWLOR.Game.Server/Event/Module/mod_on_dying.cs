using SWLOR.Game.Server;
using SWLOR.Game.Server.Event.Module;
using SWLOR.Game.Server.Messaging;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class mod_on_dying
#pragma warning restore IDE1006 // Naming Styles
    {
        // ReSharper disable once UnusedMember.Local
        public static void Main()
        {
            // Bioware Default
            SWLOR.Game.Server.NWN.NWScript.ExecuteScript("nw_o0_dying", SWLOR.Game.Server.NWN.NWScript.OBJECT_SELF); 
            MessageHub.Instance.Publish(new OnModuleDying());
        }
    }
}
