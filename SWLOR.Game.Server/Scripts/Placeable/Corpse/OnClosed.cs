using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.Corpse
{
    public class OnClosed: IScript
    {
        public void Main()
        {
            NWPlaceable container = NWScript.OBJECT_SELF;
            NWItem firstItem = NWScript.GetFirstItemInInventory(container);
            NWCreature corpseOwner = container.GetLocalObject("CORPSE_BODY");

            if (!firstItem.IsValid)
            {
                container.Destroy();
            }

            corpseOwner.AssignCommand(() =>
            {
                NWScript.SetIsDestroyable(NWScript.TRUE);
            });
        }

        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }
    }
}
