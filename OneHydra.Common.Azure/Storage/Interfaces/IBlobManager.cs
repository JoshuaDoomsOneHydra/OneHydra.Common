using System.IO;

namespace OneHydra.Common.Azure.Storage.Interfaces
{
    public interface IBlobManager
    {
        Stream GetBlobStream(string containerName, string blobFileName);
        void AddOrUpdateBlobData(string containerName, string blobFileName, string blobData);
        void DeleteBlobData(string containerName, string blobFileName);
        bool BlobExists(string containerName, string blobFileName);
    }
}
