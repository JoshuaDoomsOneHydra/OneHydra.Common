using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneHydra.Common.Utilities.Extensions;

namespace OneHydra.Common.Utilities.Test.Extensions
{
    [TestClass]
    public class DateTimeExtensionsTests
    {
        [TestMethod]
        public void MatchesCronExpressionMatches()
        {
            // Arrange 
            var now = DateTime.Now;
            // Act
            var matches = now.MatchesCronExpression("* * * * *");
            // Assert
            Assert.AreEqual(true, matches);
        }

        [TestMethod]
        public void MatchesCronExpressionDoesNotMatch()
        {
            // Arrange 
            var now = new DateTime(2001, 2, 28, 0, 0, 1);
            // Act
            var matches = now.MatchesCronExpression("1 * * * *");
            // Assert
            Assert.AreEqual(false, matches);
        }
    }
}
