﻿using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    public class StructureMode: IEntity
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public IEntity Clone()
        {
            return new StructureMode
            {
                ID = ID,
                Name = Name
            };
        }
    }
}
