using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    public class LootTable: IEntity
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public IEntity Clone()
        {
            return new LootTable
            {
                ID = ID,
                Name = Name
            };
        }
    }
}
