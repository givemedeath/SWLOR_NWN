using NWN;
using SWLOR.Game.Server.GameObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable
{
    public class PermanentVisualEffect: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable self = NWScript.OBJECT_SELF;

            int vfxID = self.GetLocalInt("PERMANENT_VFX_ID");
            
            if (vfxID > 0)
            {
                NWScript.ApplyEffectToObject(NWScript.DURATION_TYPE_PERMANENT, NWScript.EffectVisualEffect(vfxID), self);
            }

            NWScript.SetEventScript(self, NWScript.EVENT_SCRIPT_PLACEABLE_ON_HEARTBEAT, string.Empty);
        }
    }
}
