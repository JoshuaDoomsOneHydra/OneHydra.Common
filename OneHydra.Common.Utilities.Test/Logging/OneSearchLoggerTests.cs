using System;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneHydra.Common.Utilities.Configuration;
using OneHydra.Common.Utilities.Extensions;
using OneHydra.Common.Utilities.Logging;


namespace OneHydra.Common.Utilities.Test.Logging
{
    [TestClass]
    public class OneSearchLoggerTests
    {
        [TestMethod, TestCategory("Integration")]
        public void TestErrorSuccess()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var logger = new OneHydraLogger("MyTestLogger"); var helper = new ConfigManagerHelper();
            var connString = helper.GetConnectionString("OneHydraLog");
            var query = string.Format(@"
                SELECT [Id], [Date], [Thread], [Level], [Logger], [Message], [Exception], [MachineName] 
                FROM [dbo].[OneSearchLog] 
                WHERE Message LIKE {0}", guid.ForSql());
            // Act
            logger.Error(guid.ToString(), new Exception("Test exception"));
            // Clean up
            dynamic logRow;
            using (var conn = new SqlConnection(connString))
            {
                logRow = conn.GetObject(query,
                    r =>new 
                    {
                        Id = r.GetInt32(0), 
                        Date = r.GetDateTime(1), 
                        Thread = r.GetString(2), 
                        Level = r.GetString(3), 
                        Logger = r.GetString(4), 
                        Message = r.GetString(5),
                        Exception = r.GetString(6), 
                        MachineName = r.GetString(7)
                    });
                conn.ExecuteNonQuery(string.Format("DELETE FROM [dbo].[OneSearchLog] WHERE Message LIKE {0} ", guid.ForSql()));
            }
            // Assert
            Assert.IsNotNull(logRow);
            Assert.AreEqual(guid.ToString(), logRow.Message);
            Assert.AreEqual("Test exception\r\n", logRow.Exception);
            Assert.AreEqual(Environment.MachineName, logRow.MachineName);
            Assert.AreEqual("ERROR", logRow.Level);
            Assert.AreEqual("MyTestLogger", logRow.Logger);

        }
    }
}
