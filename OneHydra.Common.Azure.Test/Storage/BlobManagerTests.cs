using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneHydra.Common.Azure.Storage;
using OneHydra.Common.Azure.Storage.Interfaces;
using OneHydra.Common.Utilities.Configuration;

namespace OneHydra.Common.Azure.Test.Storage
{
    [TestClass]
    public class BlobManagerTests
    {
        [TestMethod, TestCategory("Integration")]
        public void AddEditDeleteTest()
        {
            // Arrange
            IBlobManager blobManager = new BlobManager(new ConfigManagerHelper());
            var blobContainerName = Guid.NewGuid().ToString();
            var blobFileName = Guid.NewGuid().ToString();
            var blobData = Guid.NewGuid().ToString();
            // Act
            blobManager.AddOrUpdateBlobData(blobContainerName, blobFileName, blobData);
            string blobDataRetrieved;
            using (var blobDataStream = blobManager.GetBlobStream(blobContainerName, blobFileName))
            {
                blobDataStream.Position = 0;
                using (var reader = new StreamReader(blobDataStream, Encoding.UTF8))
                {
                    blobDataRetrieved = reader.ReadToEnd();
                }
            }
            // Cleanup
            blobManager.DeleteBlobData(blobContainerName, blobFileName);
            var blobExists = blobManager.BlobExists(blobContainerName, blobFileName);
            // Assert
            Assert.AreEqual(blobData, blobDataRetrieved);
            Assert.IsFalse(blobExists);
        }
    }
}
