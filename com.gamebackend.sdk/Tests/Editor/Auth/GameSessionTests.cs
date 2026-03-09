using System;
using GameBackend.Api;
using GameBackend.Core.Interfaces;
using GameBackend.Tests.Mocks;
using NUnit.Framework;

namespace GameBackend.Tests.Auth
{
    /// <summary>
    /// Tests for GameSession (Runtime/Api/GameSession.cs).
    /// GameSession is constructed from two JWT strings (access + refresh)
    /// and decodes claims from the base64url payload.
    ///
    /// This class DOES NOT exist yet — TDD Red phase.
    /// Expected location: GameBackend.Api.GameSessionImpl : IGameSession
    /// </summary>
    [TestFixture]
    public class GameSessionTests
    {
        [Test]
        public void Constructor_ValidTokens_ExtractsUserId()
        {
            var accessToken = TestJwtHelper.CreateTestJwt(
                "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "player1", "Player One", 9999999999L);
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            Assert.AreEqual("a1b2c3d4-e5f6-7890-abcd-ef1234567890", session.UserId);
        }

        [Test]
        public void Constructor_ValidTokens_ExtractsUsername()
        {
            var accessToken = TestJwtHelper.CreateTestJwt(
                "user-id", "testplayer", "Test Player", 9999999999L);
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            Assert.AreEqual("testplayer", session.Username);
        }

        [Test]
        public void Constructor_ValidTokens_ExtractsDisplayName()
        {
            var accessToken = TestJwtHelper.CreateTestJwt(
                "user-id", "testplayer", "Test Player", 9999999999L);
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            Assert.AreEqual("Test Player", session.DisplayName);
        }

        [Test]
        public void Constructor_ValidTokens_StoresAuthToken()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            Assert.AreEqual(accessToken, session.AuthToken);
        }

        [Test]
        public void Constructor_ValidTokens_StoresRefreshToken()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            Assert.AreEqual(refreshToken, session.RefreshToken);
        }

        [Test]
        public void IsExpired_FutureExpTime_ReturnsFalse()
        {
            var accessToken = TestJwtHelper.CreateTestJwt(
                "user-id", "player1", "Player One", 9999999999L);
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            Assert.IsFalse(session.IsExpired);
        }

        [Test]
        public void IsExpired_PastExpTime_ReturnsTrue()
        {
            // exp = 1000000000 (September 2001) — definitely expired
            var accessToken = TestJwtHelper.CreateExpiredAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            Assert.IsTrue(session.IsExpired);
        }

        [Test]
        public void HasExpired_OffsetBeforeExpiry_ReturnsFalse()
        {
            var accessToken = TestJwtHelper.CreateTestJwt(
                "user-id", "player1", "Player One", 9999999999L);
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            // Check with current time — token expires far in future, so not expired
            Assert.IsFalse(session.HasExpired(DateTime.UtcNow));
        }

        [Test]
        public void HasExpired_OffsetAfterExpiry_ReturnsTrue()
        {
            // Token expires at unix 1000000000 (2001-09-09)
            var accessToken = TestJwtHelper.CreateExpiredAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            // Check with current time — token has already expired
            Assert.IsTrue(session.HasExpired(DateTime.UtcNow));
        }

        [Test]
        public void ExpireTime_ValidToken_ReturnsCorrectUnixTimestamp()
        {
            long expectedExp = 9999999999L;
            var accessToken = TestJwtHelper.CreateTestJwt(
                "user-id", "player1", "Player One", expectedExp);
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();

            var session = new GameSessionImpl(accessToken, refreshToken);

            Assert.AreEqual(expectedExp, session.ExpireTime);
        }
    }
}
