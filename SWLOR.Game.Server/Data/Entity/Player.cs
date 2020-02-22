

using SWLOR.Game.Server.Data.Contracts;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Perk;
using SWLOR.Game.Server.NWScript.Enumerations;

namespace SWLOR.Game.Server.Data.Entity
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Player : IEntity
    {
        public Player()
        {
            ID = Guid.NewGuid();
            RespawnAreaResref = "";
            Cooldowns = new Dictionary<PerkCooldownGroup, DateTime>();
            Skills = new Dictionary<Skill, PCSkill>();
            SkillPools = new Dictionary<SkillCategory, int>();

            SavedHelmets = new Dictionary<int, string>();
            SavedOutfits = new Dictionary<int, string>();
            SavedWeapons = new Dictionary<BaseItemType, Dictionary<int, string>>();

            RegionalFame = new Dictionary<FameRegion, int>();
            CraftedBlueprints = new HashSet<CraftBlueprint>();
            GuildPoints = new Dictionary<GuildType, PCGuildPoint>();
            AcquiredKeyItems = new HashSet<KeyItem>();
            MapPins = new Dictionary<string, List<PCMapPin>>();
            MapProgression = new Dictionary<string, string>();
            Perks = new Dictionary<PerkType, int>();
            OverflowItems = new Dictionary<Guid, string>();
            ObjectVisibilities = new Dictionary<string, bool>();
            ImpoundedItems = new Dictionary<Guid, PCImpoundedItem>();
            QuestStatuses = new Dictionary<int, PCQuestStatus>();
            BankItems = new Dictionary<Bank, Dictionary<Guid, BankItem>>();
        }

        [Key]
        [JsonProperty]
        public Guid ID { get; set; }
        [JsonProperty]
        public string CharacterName { get; set; }
        [JsonProperty]
        public int HitPoints { get; set; }
        [JsonProperty]
        public string LocationAreaResref { get; set; }
        [JsonProperty]
        public double LocationX { get; set; }
        [JsonProperty]
        public double LocationY { get; set; }
        [JsonProperty]
        public double LocationZ { get; set; }
        [JsonProperty]
        public double LocationOrientation { get; set; }
        [JsonProperty]
        public DateTime CreateTimestamp { get; set; }
        [JsonProperty]
        public int UnallocatedSP { get; set; }
        [JsonProperty]
        public int HPRegenerationAmount { get; set; }
        [JsonProperty]
        public int RegenerationTick { get; set; }
        [JsonProperty]
        public int RegenerationRate { get; set; }
        [JsonProperty]
        public int VersionNumber { get; set; }
        [JsonProperty]
        public int MaxFP { get; set; }
        [JsonProperty]
        public int CurrentFP { get; set; }
        [JsonProperty]
        public int CurrentFPTick { get; set; }
        [JsonProperty]
        public string RespawnAreaResref { get; set; }
        [JsonProperty]
        public double RespawnLocationX { get; set; }
        [JsonProperty]
        public double RespawnLocationY { get; set; }
        [JsonProperty]
        public double RespawnLocationZ { get; set; }
        [JsonProperty]
        public double RespawnLocationOrientation { get; set; }
        [JsonProperty]
        public DateTime DateSanctuaryEnds { get; set; }
        [JsonProperty]
        public bool IsSanctuaryOverrideEnabled { get; set; }
        [JsonProperty]
        public int STRBase { get; set; }
        [JsonProperty]
        public int DEXBase { get; set; }
        [JsonProperty]
        public int CONBase { get; set; }
        [JsonProperty]
        public int INTBase { get; set; }
        [JsonProperty]
        public int WISBase { get; set; }
        [JsonProperty]
        public int CHABase { get; set; }
        [JsonProperty]
        public int TotalSPAcquired { get; set; }
        [JsonProperty]
        public bool DisplayHelmet { get; set; }
        [JsonProperty]
        public Guid? PrimaryResidencePCBaseStructureID { get; set; }
        [JsonProperty]
        public DateTime? DatePerkRefundAvailable { get; set; }
        [JsonProperty]
        public AssociationType AssociationID { get; set; }
        [JsonProperty]
        public bool DisplayHolonet { get; set; }
        [JsonProperty]
        public bool DisplayDiscord { get; set; }
        [JsonProperty]
        public Guid? PrimaryResidencePCBaseID { get; set; }
        [JsonProperty]
        public bool IsUsingNovelEmoteStyle { get; set; }
        [JsonProperty]
        public bool IsDeleted { get; set; }
        [JsonProperty]
        public int XPBonus { get; set; }
        [JsonProperty]
        public int LeaseRate { get; set; }
        [JsonProperty]
        public Guid? LocationInstanceID { get; set; }
        [JsonProperty]
        public int GoldTill { get; set; }
        [JsonProperty]
        public int RoleplayPoints { get; set; }
        [JsonProperty]
        public int RoleplayXP { get; set; }
        [JsonProperty]
        public SpecializationType SpecializationID { get; set; }
        [JsonProperty]
        public int? ActiveConcentrationPerkID { get; set; }
        [JsonProperty]
        public int ActiveConcentrationTier { get; set; }
        [JsonProperty]
        public Dictionary<PerkCooldownGroup, DateTime> Cooldowns { get; set; }
        [JsonProperty]
        public Dictionary<Skill, PCSkill> Skills { get; set; }
        [JsonProperty]
        public Dictionary<SkillCategory, int> SkillPools { get; set; }
        [JsonProperty]
        public Dictionary<int, string> SavedOutfits { get; set; }
        [JsonProperty]
        public Dictionary<BaseItemType, Dictionary<int, string>> SavedWeapons { get; set; }
        [JsonProperty]
        public Dictionary<int, string> SavedHelmets { get; set; }
        [JsonProperty]
        public Dictionary<FameRegion, int> RegionalFame { get; set; }
        [JsonProperty]
        public HashSet<CraftBlueprint> CraftedBlueprints { get; set; }
        [JsonProperty]
        public Dictionary<GuildType, PCGuildPoint> GuildPoints { get; set; }
        [JsonProperty]
        public HashSet<KeyItem> AcquiredKeyItems { get; set; }
        [JsonProperty]
        public Dictionary<string, List<PCMapPin>> MapPins { get; set; }
        [JsonProperty]
        public Dictionary<string, string> MapProgression { get; set; }
        [JsonProperty]
        public Dictionary<PerkType, int> Perks { get; set; }
        [JsonProperty]
        public Dictionary<Guid, string> OverflowItems { get; set; }
        [JsonProperty]
        public Dictionary<string, bool> ObjectVisibilities { get; set; }
        [JsonProperty]
        public Dictionary<Guid, PCImpoundedItem> ImpoundedItems { get; set; }
        [JsonProperty]
        public Dictionary<int, PCQuestStatus> QuestStatuses { get; set; }
        [JsonProperty]
        public Dictionary<Bank, Dictionary<Guid, BankItem>> BankItems { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PCSkill
    {
        [JsonProperty]
        public int XP { get; set; }
        [JsonProperty]
        public int Rank { get; set; }
        [JsonProperty]
        public bool IsLocked { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class PCGuildPoint
    {
        [JsonProperty]
        public int Rank { get; set; }
        [JsonProperty]
        public int Points { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PCMapPin
    {
        [JsonProperty]
        public double PositionX { get; set; }
        [JsonProperty]
        public double PositionY { get; set; }
        [JsonProperty]
        public string NoteText { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PCImpoundedItem
    {
        [JsonProperty]
        public string ItemName { get; set; }
        [JsonProperty]
        public string ItemTag { get; set; }
        [JsonProperty]
        public string ItemResref { get; set; }
        [JsonProperty]
        public string ItemObject { get; set; }
        [JsonProperty]
        public DateTime DateImpounded { get; set; }
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class PCQuestStatus
    {
        public PCQuestStatus()
        {
            KillTargets = new Dictionary<NPCGroup, int>();
            Items = new Dictionary<string, PCQuestItemProgress>();
        }
        [JsonProperty]
        public int QuestState { get; set; }
        [JsonProperty]
        public DateTime? CompletionDate { get; set; }
        [JsonProperty]
        public int TimesCompleted { get; set; }

        [JsonProperty]
        public Dictionary<NPCGroup, int> KillTargets { get; set; }
        [JsonProperty]
        public Dictionary<string, PCQuestItemProgress> Items { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PCQuestItemProgress
    {
        [JsonProperty]
        public int Remaining { get; set; }
        [JsonProperty]
        public bool MustBeCraftedByPlayer { get; set; }

        public PCQuestItemProgress(int remaining, bool mustBeCraftedByPlayer)
        {
            Remaining = remaining;
            MustBeCraftedByPlayer = mustBeCraftedByPlayer;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class BankItem
    {
        [JsonProperty]
        public string ItemID { get; set; }
        [JsonProperty]
        public string ItemName { get; set; }
        [JsonProperty]
        public string ItemTag { get; set; }
        [JsonProperty]
        public string ItemResref { get; set; }
        [JsonProperty]
        public string ItemObject { get; set; }
        [JsonProperty]
        public DateTime DateStored { get; set; }
    }
}
