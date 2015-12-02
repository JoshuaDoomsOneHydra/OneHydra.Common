using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace OneHydra.Common.Azure.Storage.Interfaces
{
    public interface ITableManager
    {
        IEnumerable<T> Get<T>(string tableName, string partitionKey) where T : TableEntity, new();
        T Get<T>(string tableName, string partitionKey, string rowKey) where T : TableEntity, new();
        void InsertOrMerge<T>(string tableName, T entity) where T : TableEntity;
        void Delete<T>(string tableName, T entity) where T : TableEntity;
        void InsertOrMerge<T>(string tableName, IEnumerable<T> entities) where T : TableEntity;
        void Delete<T>(string tableName, IEnumerable<T> entities) where T : TableEntity;
    }
}
