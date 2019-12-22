using System.Collections.Generic;
using SWLOR.Game.Server.Data.Entity;

namespace SWLOR.Game.Server.Caching
{
    public class LootTableItemCache: CacheBase<LootTableItem>
    {
        private Dictionary<int, Dictionary<int, LootTableItem>> ByLootTableID { get; } = new Dictionary<int, Dictionary<int, LootTableItem>>();

        protected override void OnCacheObjectSet(string @namespace, object id, LootTableItem entity)
        {
            SetEntityIntoDictionary(entity.LootTableID, entity.ID, entity, ByLootTableID);
        }

        protected override void OnCacheObjectRemoved(string @namespace, object id, LootTableItem entity)
        {
            RemoveEntityFromDictionary(entity.LootTableID, entity.ID, ByLootTableID);
        }

        protected override void OnSubscribeEvents()
        {
        }

        public LootTableItem GetByID(int id)
        {
            return ByID(id);
        }

        public IEnumerable<LootTableItem> GetAllByLootTableID(int lootTableID)
        {
            var list = new List<LootTableItem>();
            if (!ByLootTableID.ContainsKey(lootTableID))
                return list;

            foreach (var table in ByLootTableID[lootTableID].Values)
            {
                list.Add((LootTableItem) table.Clone());
            }

            return list;
        }
    }
}
