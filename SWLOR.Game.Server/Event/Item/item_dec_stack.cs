using SWLOR.Game.Server.Event.Item;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Messaging;
using SWLOR.Game.Server.NWNX;

// ReSharper disable once CheckNamespace
namespace NWN.Scripts
{
#pragma warning disable IDE1006 // Naming Styles
    public class item_dec_stack
#pragma warning restore IDE1006 // Naming Styles
    {
        // ReSharper disable once UnusedMember.Local
        public static void Main()
        {
            if (SWLOR.Game.Server.NWN.NWScript.OBJECT_SELF == null) return;

            NWItem item = SWLOR.Game.Server.NWN.NWScript.OBJECT_SELF;
            if (!item.IsValid) return;

            // We ignore any decrements to shurikens, darts, and throwing axes.
            if(item.BaseItemType == SWLOR.Game.Server.NWN.NWScript.BASE_ITEM_SHURIKEN ||
               item.BaseItemType == SWLOR.Game.Server.NWN.NWScript.BASE_ITEM_DART ||
               item.BaseItemType == SWLOR.Game.Server.NWN.NWScript.BASE_ITEM_THROWINGAXE)
            {
                NWNXEvents.SkipEvent();
            }

            MessageHub.Instance.Publish(new OnItemDecrementStack(), false);
        }
    }
}