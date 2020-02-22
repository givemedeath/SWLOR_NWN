using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;

namespace SWLOR.Game.Server.Event.SWLOR
{
    public class OnStoreBankItem
    {
        public NWPlayer Player { get; set; }
        public Bank BankID { get; set; }
        public BankItem Entity { get; set; }

        public OnStoreBankItem(NWPlayer player, Bank bankID, BankItem entity)
        {
            Player = player;
            BankID = bankID;
            Entity = entity;
        }
    }
}
