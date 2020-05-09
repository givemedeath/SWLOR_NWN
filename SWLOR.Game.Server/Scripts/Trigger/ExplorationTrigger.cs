using System;
using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Trigger
{
    public class ExplorationTrigger: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWCreature oPC = (NWScript.GetEnteringObject());
            if (!oPC.IsPlayer) return;

            string triggerID = NWScript.GetLocalString(NWScript.OBJECT_SELF, "TRIGGER_ID");
            if (string.IsNullOrWhiteSpace(triggerID))
            {
                triggerID = Guid.NewGuid().ToString();
                NWScript.SetLocalString(NWScript.OBJECT_SELF, "TRIGGER_ID", triggerID);
            }

            if (NWScript.GetLocalInt(oPC.Object, triggerID) == 1) return;

            string message = NWScript.GetLocalString(NWScript.OBJECT_SELF, "DISPLAY_TEXT");
            NWScript.SendMessageToPC(oPC.Object, ColorTokenService.Cyan(message));
            NWScript.SetLocalInt(oPC.Object, triggerID, 1);

            NWScript.AssignCommand(oPC.Object, () => NWScript.PlaySound("gui_prompt"));

        }
    }
}
