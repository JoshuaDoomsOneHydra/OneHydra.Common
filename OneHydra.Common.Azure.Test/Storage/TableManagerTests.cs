using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;
using OneHydra.Common.Azure.Storage;
using OneHydra.Common.Azure.Storage.Interfaces;
using OneHydra.Common.Utilities.Configuration;

namespace OneHydra.Common.Azure.Test.Storage
{

    public class DiscoveredKeywordEntity : TableEntity
    {
        public int OccurrenceInFeed { get; set; }
        public string Text { get; set; }
        public int Volume { get; set; }
    }

    [TestClass]
    public class TableManagerTests
    {
        [TestMethod, TestCategory("Integration")]
        public void AddGetDeleteSingleEntityTest()
        {
            // Arrange
            ITableManager tableManager = new TableManager(new ConfigManagerHelper());
            const string testTableName = "Test";
            var entity = new DiscoveredKeywordEntity
            {
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = Guid.NewGuid().ToString(),
                OccurrenceInFeed = 2,
                Text = Guid.NewGuid().ToString(),
                Volume = 10
            };
            // Act
            tableManager.InsertOrMerge(testTableName, entity);
            var entityRetrieved = tableManager.Get<DiscoveredKeywordEntity>(testTableName, entity.PartitionKey, entity.RowKey);
            // Cleanup
            tableManager.Delete(testTableName, entity);
            var entityRetrievedAfterDelete = tableManager.Get<DiscoveredKeywordEntity>(testTableName, entity.PartitionKey, entity.RowKey);
            // Assert
            Assert.IsNotNull(entityRetrieved);
            Assert.IsNull(entityRetrievedAfterDelete);
            Assert.AreEqual(entity.PartitionKey, entityRetrieved.PartitionKey);
            Assert.AreEqual(entity.RowKey, entityRetrieved.RowKey);
            Assert.AreEqual(entity.OccurrenceInFeed, entityRetrieved.OccurrenceInFeed);
            Assert.AreEqual(entity.Volume, entityRetrieved.Volume);
            Assert.AreEqual(entity.Text, entityRetrieved.Text);
        }

        [TestMethod, TestCategory("Integration")]
        public void AddGetDeleteMultipleEntityTest()
        {
            // Arrange
            ITableManager tableManager = new TableManager(new ConfigManagerHelper());
            const string testTableName = "Test";
            var partitionKey = Guid.NewGuid().ToString();
            var entities = new List<DiscoveredKeywordEntity>
            {
                new DiscoveredKeywordEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = Guid.NewGuid().ToString(),
                    OccurrenceInFeed = 2,
                    Text = Guid.NewGuid().ToString(),
                    Volume = 10
                },
                new DiscoveredKeywordEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = Guid.NewGuid().ToString(),
                    OccurrenceInFeed = 2,
                    Text = Guid.NewGuid().ToString(),
                    Volume = 10
                }
            };
            // Act
            tableManager.InsertOrMerge<DiscoveredKeywordEntity>(testTableName, entities);
            var entitiesRetrieved = tableManager.Get<DiscoveredKeywordEntity>(testTableName, partitionKey).ToList();
            // Cleanup
            tableManager.Delete<DiscoveredKeywordEntity>(testTableName, entities);
            var entitiesRetrievedAfterDelete = tableManager.Get<DiscoveredKeywordEntity>(testTableName, partitionKey).ToList();
            // Assert
            Assert.AreEqual(entities.Count, entitiesRetrieved.Count());
            Assert.AreEqual(0, entitiesRetrievedAfterDelete.Count());
            for (var x = 0; x < entities.Count; x++)
            {
                Assert.AreEqual(entities[x].PartitionKey, entitiesRetrieved[x].PartitionKey);
                Assert.AreEqual(entities[x].RowKey, entitiesRetrieved[x].RowKey);
                Assert.AreEqual(entities[x].OccurrenceInFeed, entitiesRetrieved[x].OccurrenceInFeed);
                Assert.AreEqual(entities[x].Volume, entitiesRetrieved[x].Volume);
                Assert.AreEqual(entities[x].Text, entitiesRetrieved[x].Text);
            }
        }
    }
}
