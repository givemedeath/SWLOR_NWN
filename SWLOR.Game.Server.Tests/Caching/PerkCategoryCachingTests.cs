using System.Collections.Generic;
using NUnit.Framework;
using SWLOR.Game.Server.Caching;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Event.SWLOR;
using SWLOR.Game.Server.Messaging;

namespace SWLOR.Game.Server.Tests.Caching
{
    public class PerkCategoryCacheTests
    {
        private PerkCategoryCache _cache;

        [SetUp]
        public void Setup()
        {
            _cache = new PerkCategoryCache();
        }

        [TearDown]
        public void TearDown()
        {
            _cache = null;
        }


        [Test]
        public void GetByID_OneItem_ReturnsPerkCategory()
        {
            // Arrange
            var entity = new PerkCategory {ID = 1};
            
            // Act
            MessageHub.Instance.Publish(new OnCacheObjectSet<PerkCategory>(entity));

            // Assert
            Assert.AreNotSame(entity, _cache.GetByID(1));
        }

        [Test]
        public void GetByID_TwoItems_ReturnsCorrectObject()
        {
            // Arrange
            var entity1 = new PerkCategory { ID = 1};
            var entity2 = new PerkCategory { ID = 2};

            // Act
            MessageHub.Instance.Publish(new OnCacheObjectSet<PerkCategory>(entity1));
            MessageHub.Instance.Publish(new OnCacheObjectSet<PerkCategory>(entity2));

            // Assert
            Assert.AreNotSame(entity1, _cache.GetByID(1));
            Assert.AreNotSame(entity2, _cache.GetByID(2));
        }

        [Test]
        public void GetByID_RemovedItem_ReturnsCorrectObject()
        {
            // Arrange
            var entity1 = new PerkCategory { ID = 1};
            var entity2 = new PerkCategory { ID = 2};

            // Act
            MessageHub.Instance.Publish(new OnCacheObjectSet<PerkCategory>(entity1));
            MessageHub.Instance.Publish(new OnCacheObjectSet<PerkCategory>(entity2));
            MessageHub.Instance.Publish(new OnCacheObjectDeleted<PerkCategory>(entity1));

            // Assert
            Assert.Throws<KeyNotFoundException>(() => { _cache.GetByID(1); });
            Assert.AreNotSame(entity2, _cache.GetByID(2));
        }

        [Test]
        public void GetByID_NoItems_ThrowsKeyNotFoundException()
        {
            // Arrange
            var entity1 = new PerkCategory { ID = 1};
            var entity2 = new PerkCategory { ID = 2};

            // Act
            MessageHub.Instance.Publish(new OnCacheObjectSet<PerkCategory>(entity1));
            MessageHub.Instance.Publish(new OnCacheObjectSet<PerkCategory>(entity2));
            MessageHub.Instance.Publish(new OnCacheObjectDeleted<PerkCategory>(entity1));
            MessageHub.Instance.Publish(new OnCacheObjectDeleted<PerkCategory>(entity2));

            // Assert
            Assert.Throws<KeyNotFoundException>(() => { _cache.GetByID(1); });
            Assert.Throws<KeyNotFoundException>(() => { _cache.GetByID(2); });

        }
    }
}
