﻿using System;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Enumeration;

namespace SWLOR.Game.Server.Data.Entity
{
    public class PCMarketListing: IEntity
    {
        public PCMarketListing()
        {
            ID = Guid.NewGuid();
        }

        [Key]
        public Guid ID { get; set; }
        public Guid SellerPlayerID { get; set; }
        public string Note { get; set; }
        public int Price { get; set; }
        public int MarketRegionID { get; set; }
        public MarketCategory MarketCategoryID { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime DateExpires { get; set; }
        public DateTime? DateSold { get; set; }
        public DateTime? DateRemoved { get; set; }
        public Guid? BuyerPlayerID { get; set; }
        public string ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemTag { get; set; }
        public string ItemResref { get; set; }
        public string ItemObject { get; set; }
        public int ItemRecommendedLevel { get; set; }
        public int ItemStackSize { get; set; }
    }
}
