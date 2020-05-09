using SWLOR.Game.Server;

using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.ValueObject;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class open_store
#pragma warning restore IDE1006 // Naming Styles
    {
        public static void Main()
        {
            using (new Profiler(nameof(open_store)))
            {
                NWPlayer player = SWLOR.Game.Server.NWN.NWScript.GetPCSpeaker();
                NWObject self = SWLOR.Game.Server.NWN.NWScript.OBJECT_SELF;
                string storeTag = self.GetLocalString("STORE_TAG");
                NWObject store;

                if (string.IsNullOrWhiteSpace(storeTag))
                {
                    store = SWLOR.Game.Server.NWN.NWScript.GetNearestObject(SWLOR.Game.Server.NWN.NWScript.OBJECT_TYPE_STORE, self);
                }
                else
                {
                    store = SWLOR.Game.Server.NWN.NWScript.GetObjectByTag(storeTag);
                }

                if (!store.IsValid)
                {
                    SWLOR.Game.Server.NWN.NWScript.SpeakString("ERROR: Unable to locate store.");
                }

                SWLOR.Game.Server.NWN.NWScript.OpenStore(store, player);
            }
        }
    }
}
