using System;
using NUnit.Framework;
using GameBackend.Core.Exceptions;

namespace GameBackend.Tests.Core.Exceptions
{
    [TestFixture]
    public class GameApiExceptionTests
    {
        [Test]
        public void GameApiException_Constructor_SetsProperties()
        {
            var exception = new GameApiException(403, "Account is banned");

            Assert.AreEqual(403, exception.StatusCode);
            Assert.AreEqual("Account is banned", exception.ErrorMessage);
        }

        [Test]
        public void GameApiException_InheritsFromException()
        {
            var exception = new GameApiException(500, "Internal server error");

            Assert.IsInstanceOf<Exception>(exception);
        }

        [Test]
        public void GameApiException_NullableFields_DefaultToNull()
        {
            var exception = new GameApiException(404, "Not found");

            Assert.IsNull(exception.ErrorCode);
            Assert.IsNull(exception.RequestId);
        }
    }
}
