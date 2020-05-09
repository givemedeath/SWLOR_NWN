using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.Stores
{
    public class GenericStore: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable self = (NWScript.OBJECT_SELF);
            NWObject oPC = (NWScript.GetLastUsedBy());
            string storeTag = self.GetLocalString("STORE_TAG");
            uint store = NWScript.GetObjectByTag(storeTag);

            NWScript.OpenStore(store, oPC.Object);
        }
    }
}
