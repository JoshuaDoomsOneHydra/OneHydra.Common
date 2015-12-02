using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneHydra.Common.Utilities.Configuration;

namespace OneHydra.Common.Utilities.Test.Configuration
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void TestGetConnectionString()
        {
            var helper = new ConfigManagerHelper();
            var connString = helper.GetConnectionString("OneHydra");
            Assert.AreEqual("Server=onehydra-dev.database.windows.net;initial catalog=OneSearch_Dev;User ID=Greenlight@onehydra-dev;Password=1poldo1.;Trusted_Connection=False;Persist Security Info=True;", connString);
        }

        [TestMethod]
        public void TestGetAppSetting()
        {
            var helper = new ConfigManagerHelper();
            var connString = helper.GetAppSetting("DataConnectionString");
            Assert.AreEqual("AccountName=onehydratest;AccountKey=jh50v102CF9DMU71vNoel0O0csTmsXfhasPWUL/tUCZ7icu0z2QMPV0rLOXN+9dTmgQHyFzOUFOZdY+Cf01sTA==;DefaultEndpointsProtocol=https", connString);
        }
    }
}
