using System;
using System.Linq;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Event.Module;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Messaging;


namespace SWLOR.Game.Server.Service
{
    public static class ImpoundService
    {
        public static void SubscribeEvents()
        {
            MessageHub.Instance.Subscribe<OnModuleLoad>(message => PruneOldImpoundItems());
        }

        private static void PruneOldImpoundItems()
        {
            var appSettings = ApplicationSettings.Get();

            if (appSettings.ImpoundPruneDays <= 0)
            {
                Console.WriteLine("Impound item pruning is disabled. Set the SWLOR_IMPOUND_PRUNE_DAYS environment variable if you wish to enable this.");
                return;
            }

            var now = DateTime.UtcNow;

            var players = DataService.Player.GetAll();
            int itemsPruned = 0;
            foreach (var player in players)
            {
                for (int x = player.ImpoundedItems.Count-1; x >= 0; x--)
                {
                    var item = player.ImpoundedItems.ElementAt(x);
                    if((now - item.Value.DateImpounded).TotalDays >= appSettings.ImpoundPruneDays)
                    {
                        player.ImpoundedItems.Remove(item.Key);
                        itemsPruned++;
                    }
                }

                DataService.Set(player);
            }

            Console.WriteLine($"{itemsPruned} impounded items have been pruned.");
        }

        public static void Impound(Guid pcBaseStructureID, PCBaseStructureItem pcBaseStructureItem)
        {
            var pcBaseStructure = DataService.PCBaseStructure.GetByID(pcBaseStructureID);
            var pcBase = DataService.PCBase.GetByID(pcBaseStructure.PCBaseID);
            var player = DataService.Player.GetByID(pcBase.PlayerID);

            var impoundItem = new PCImpoundedItem
            {
                DateImpounded = DateTime.UtcNow,
                ItemName = pcBaseStructureItem.ItemName,
                ItemResref = pcBaseStructureItem.ItemResref,
                ItemObject = pcBaseStructureItem.ItemObject,
                ItemTag = pcBaseStructureItem.ItemTag
            };
            player.ImpoundedItems.Add(Guid.NewGuid(), impoundItem);
            DataService.Set(player);
        }

        public static void Impound(Guid playerID, NWItem item)
        {
            var player = DataService.Player.GetByID(playerID);
            PCImpoundedItem structureImpoundedItem = new PCImpoundedItem
            {
                DateImpounded = DateTime.UtcNow,
                ItemObject = SerializationService.Serialize(item),
                ItemTag = item.Tag,
                ItemResref = item.Resref,
                ItemName = item.Name
            };

            player.ImpoundedItems.Add(Guid.NewGuid(), structureImpoundedItem);
            DataService.Set(player);
        }
    }
}
