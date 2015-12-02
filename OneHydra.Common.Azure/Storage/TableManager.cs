using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using OneHydra.Common.Azure.Storage.Interfaces;
using OneHydra.Common.Utilities.Configuration;
using OneHydra.Common.Utilities.Extensions;

namespace OneHydra.Common.Azure.Storage
{
    public class TableManager : ITableManager
    {
        private readonly CloudTableClient _tableClient;
        
        public TableManager(IConfigManagerHelper config)
        {
            var azureDataConnectionString = config.GetAppSetting("DataConnectionString");
            var storageAccount = CloudStorageAccount.Parse(azureDataConnectionString);
            _tableClient = storageAccount.CreateCloudTableClient();
        }

        public IEnumerable<T> Get<T>(string tableName, string partitionKey) where T : TableEntity, new()
        {
            IEnumerable<T> returnValue = new List<T>();
            var azureTable = _tableClient.GetTableReference(tableName);
            if (azureTable != null && azureTable.Exists())
            {
                var tableQuery = new TableQuery<T>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
                returnValue = azureTable.ExecuteQuery(tableQuery);
            }
            return returnValue;
        }

        public T Get<T>(string tableName, string partitionKey, string rowKey) where T : TableEntity, new()
        {
            var returnValue = default(T);
            var azureTable = _tableClient.GetTableReference(tableName);
            var tableResult = azureTable.Execute(TableOperation.Retrieve<T>(partitionKey, rowKey));
            if (tableResult.Result != null)
            {
                returnValue = tableResult.Result.CastTo<T>();
            }
            return returnValue;
        }

        public void InsertOrMerge<T>(string tableName, T entity) where T : TableEntity
        {
            var azureTable = _tableClient.GetTableReference(tableName);
            azureTable.CreateIfNotExists();
            azureTable.Execute(TableOperation.InsertOrMerge(entity));
        }

        public void Delete<T>(string tableName, T entity) where T : TableEntity
        {
            var azureTable = _tableClient.GetTableReference(tableName);
            azureTable.Execute(TableOperation.Delete(entity));
        }

        public void InsertOrMerge<T>(string tableName, IEnumerable<T> entities) where T : TableEntity
        {
            var azureTable = _tableClient.GetTableReference(tableName);
            azureTable.CreateIfNotExists();
            foreach (var entity in entities)
            {
                azureTable.Execute(TableOperation.InsertOrMerge(entity));
            }
        }

        public void Delete<T>(string tableName, IEnumerable<T> entities) where T : TableEntity
        {
            var azureTable = _tableClient.GetTableReference(tableName);
            foreach (var entity in entities)
            {
                azureTable.Execute(TableOperation.Delete(entity));
            }
        }
    }
}
