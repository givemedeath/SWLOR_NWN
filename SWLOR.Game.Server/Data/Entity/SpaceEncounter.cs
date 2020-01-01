using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    public class SpaceEncounter: IEntity
    {
        [Key]
        public int ID { get; set; }
        public string Planet { get; set; }
        public int TypeID { get; set; }
        public int Chance { get; set; }
        public int Difficulty { get; set; }
        public int LootTableID { get; set; }
    }
}
