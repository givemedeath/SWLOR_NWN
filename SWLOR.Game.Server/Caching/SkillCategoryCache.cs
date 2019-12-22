using System.Collections.Generic;
using SWLOR.Game.Server.Data.Entity;

namespace SWLOR.Game.Server.Caching
{
    public class SkillCategoryCache: CacheBase<SkillCategory>
    {
        private Dictionary<int, SkillCategory> ByActive { get; } = new Dictionary<int, SkillCategory>();

        protected override void OnCacheObjectSet(string @namespace, object id, SkillCategory entity)
        {
            SetByActive(entity);
        }

        protected override void OnCacheObjectRemoved(string @namespace, object id, SkillCategory entity)
        {
            ByActive.Remove(entity.ID);
        }

        protected override void OnSubscribeEvents()
        {
        }

        private void SetByActive(SkillCategory entity)
        {
            // Exclude inactive / remove if swapped to inactive.
            if (!entity.IsActive && ByActive.ContainsKey(entity.ID))
            {
                ByActive.Remove(entity.ID);
                return;
            }

            // Exclude zero
            if (entity.ID <= 0) return;

            ByActive[entity.ID] = (SkillCategory)entity.Clone();
        }

        public SkillCategory GetByID(int id)
        {
            return ByID(id);
        }

        public IEnumerable<SkillCategory> GetAllActive()
        {
            var list = new List<SkillCategory>();
            foreach (var record in ByActive.Values)
            {
                list.Add((SkillCategory) record.Clone());
            }

            return list;
        }

    }
}
