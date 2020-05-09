using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable
{
    public class GenericConversation: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable placeable = (NWScript.OBJECT_SELF);
            NWPlayer user = placeable.ObjectType == ObjectType.Placeable ?
                NWScript.GetLastUsedBy() :
                NWScript.GetClickingObject();

            if (!user.IsPlayer && !user.IsDM) return;

            string conversation = placeable.GetLocalString("CONVERSATION");
            NWObject target = placeable.GetLocalInt("TARGET_PC") == NWScript.TRUE ? user.Object : placeable.Object;

            if (!string.IsNullOrWhiteSpace(conversation))
            {
                DialogService.StartConversation(user, target, conversation);
            }
            else
            {
                user.AssignCommand(() => NWScript.ActionStartConversation(target, string.Empty, NWScript.TRUE, NWScript.FALSE));
            }

        }
    }
}
