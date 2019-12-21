using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    public class SkillCategory: IEntity
    {
        public SkillCategory()
        {
            Name = "";
        }

        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int Sequence { get; set; }

        public IEntity Clone()
        {
            return new SkillCategory
            {
                ID = ID,
                Name = Name,
                IsActive = IsActive,
                Sequence = Sequence
            };
        }
    }
}
