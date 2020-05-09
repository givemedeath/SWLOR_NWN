
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.ValueObject;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class credit_check
#pragma warning restore IDE1006 // Naming Styles
    {
        public static int Main()
        {
            using (new Profiler(nameof(credit_check)))
            {
                NWPlayer oPC = SWLOR.Game.Server.NWN.NWScript.GetPCSpeaker();
                NWObject oNPC = SWLOR.Game.Server.NWN.NWScript.OBJECT_SELF;
                int nGold = SWLOR.Game.Server.NWN.NWScript.GetGold(oPC);
                int reqGold = SWLOR.Game.Server.NWN.NWScript.GetLocalInt(oNPC, "gold");
                if (nGold > reqGold)
                {
                    return SWLOR.Game.Server.NWN.NWScript.TRUE;
                }

                return SWLOR.Game.Server.NWN.NWScript.FALSE;
            }
        }
    }
}
