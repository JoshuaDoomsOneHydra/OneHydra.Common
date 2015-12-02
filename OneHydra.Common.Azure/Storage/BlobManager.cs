using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using OneHydra.Common.Azure.Storage.Interfaces;
using OneHydra.Common.Utilities.Configuration;
using OneHydra.Common.Utilities.Extensions;

namespace OneHydra.Common.Azure.Storage
{
    public class BlobManager : IBlobManager
    {
        private readonly CloudBlobClient _blobClient;

        public BlobManager(IConfigManagerHelper config)
        {
            var azureDataConnectionString = config.GetAppSetting("DataConnectionString");
            var storageAccount = CloudStorageAccount.Parse(azureDataConnectionString);
            _blobClient = storageAccount.CreateCloudBlobClient();
        }

        public Stream GetBlobStream(string containerName, string blobFileName)
        {
            Stream returnStream = null;
            var blobContainer = _blobClient.GetContainerReference(containerName);
            if (blobContainer != null && blobContainer.Exists())
            {
                var blob = blobContainer.GetBlockBlobReference(blobFileName);
                if (blob != null && blob.Exists())
                {
                    //use ICloudBlob.DownloadToStream() instead of ICloudBlob.OpenRead(). This can avoid concurrency issue in higher level code.
                    //The reason is that ICloudBlob.OpenRead() and a later call to Stream.ReadToEnd(), returned from ICloudBlob.OpenRead(), will 
                    //use the same ETag for two seperate requests. If an update between these two requests and resulted in an updated ETag in
                    //server, HTTP 412 error will be thrown from server for the second request.
                    Stream targetStream = new MemoryStream();
                    blob.DownloadToStream(targetStream);
                    targetStream.Position = 0;
                    returnStream = targetStream;
                }
            }
            return returnStream;
        }

        public void AddOrUpdateBlobData(string containerName, string blobFileName, string blobData)
        {
            var blobContainer = _blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExists();
            var blob = blobContainer.GetBlockBlobReference(blobFileName);
            if (blob != null)
            {
                using (var dataStream = blobData.AsStream())
                {
                    blob.UploadFromStream(dataStream);
                }
            }
            else
            {
                throw new Exception(string.Format("A blob reference could not be created for container:{0} and blob:{1}", containerName, blobFileName));
            }
        }

        public void DeleteBlobData(string containerName, string blobFileName)
        {
            var blobContainer = _blobClient.GetContainerReference(containerName);
            if (blobContainer != null && blobContainer.Exists())
            {
                var blob = blobContainer.GetBlockBlobReference(blobFileName);
                if (blob != null && blob.Exists())
                {
                    blob.Delete();
                }
            }
        }

        public bool BlobExists(string containerName, string blobFileName)
        {
            var blobExists = false;
            var blobContainer = _blobClient.GetContainerReference(containerName);
            if (blobContainer != null && blobContainer.Exists())
            {
                blobContainer.CreateIfNotExists();
                var blob = blobContainer.GetBlockBlobReference(blobFileName);
                blobExists = (blob != null && blob.Exists());
            }
            return blobExists;
        }
    }
}
