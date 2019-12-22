using System;
using System.Collections.Generic;
using SWLOR.Game.Server.Data.Entity;

namespace SWLOR.Game.Server.Caching
{
    public class PCGuildPointCache: CacheBase<PCGuildPoint>
    {
        private Dictionary<Guid, Dictionary<int, PCGuildPoint>> ByPlayerIDAndGuildID { get; } = new Dictionary<Guid, Dictionary<int, PCGuildPoint>>();

        protected override void OnCacheObjectSet(string @namespace, object id, PCGuildPoint entity)
        {
            SetEntityIntoDictionary(entity.PlayerID, entity.GuildID, entity, ByPlayerIDAndGuildID);
        }

        protected override void OnCacheObjectRemoved(string @namespace, object id, PCGuildPoint entity)
        {
            RemoveEntityFromDictionary(entity.PlayerID, entity.GuildID, ByPlayerIDAndGuildID);
        }

        protected override void OnSubscribeEvents()
        {
        }

        public PCGuildPoint GetByID(Guid id)
        {
            return ByID(id);
        }

        public PCGuildPoint GetByIDOrDefault(Guid id)
        {
            if (!Exists(id))
                return default;
            return ByID(id);
        }

        public PCGuildPoint GetByPlayerIDAndGuildID(Guid playerID, int guildID)
        {
            return GetEntityFromDictionary(playerID, guildID, ByPlayerIDAndGuildID);
        }

        public PCGuildPoint GetByPlayerIDAndGuildIDOrDefault(Guid playerID, int guildID)
        {
            return GetEntityFromDictionaryOrDefault(playerID, guildID, ByPlayerIDAndGuildID);
        }
    }
}
