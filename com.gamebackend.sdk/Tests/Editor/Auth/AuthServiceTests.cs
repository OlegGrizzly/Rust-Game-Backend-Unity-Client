using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameBackend.Api;
using GameBackend.Core.Exceptions;
using GameBackend.Core.Interfaces;
using GameBackend.Core.Models;
using GameBackend.Tests.Mocks;
using GameBackend.Transport;
using GameBackend.Transport.Interfaces;
using NUnit.Framework;

namespace GameBackend.Tests.Auth
{
    /// <summary>
    /// Tests for AuthService via MockHttpAdapter.
    /// These tests define the contract for auth operations (TDD Red phase).
    ///
    /// Dependencies that DO NOT exist yet (will be created by AUTH agent):
    ///   - GameBackend.Api.AuthService : IAuthClient
    ///   - GameBackend.Api.TokenManager (manages current session, auto-refresh)
    ///   - GameBackend.Api.HttpPipeline (sends requests through IHttpAdapter with auth headers)
    ///   - GameBackend.Transport.NewtonsoftSerializer : ISerializer
    ///
    /// Test structure:
    ///   1. Create MockHttpAdapter + MockTokenStorage
    ///   2. Create real ISerializer (NewtonsoftSerializer)
    ///   3. Create TokenManager(ISerializer, ITokenStorage)
    ///   4. Create HttpPipeline(IHttpAdapter, ISerializer, TokenManager)
    ///   5. Create AuthService(HttpPipeline, TokenManager, baseUrl)
    ///   6. Enqueue responses, call method, assert results + SentRequests
    /// </summary>
    [TestFixture]
    public class AuthServiceTests
    {
        private MockHttpAdapter _httpAdapter;
        private MockTokenStorage _tokenStorage;
        private ISerializer _serializer;
        private TokenManager _tokenManager;
        private HttpPipeline _pipeline;
        private AuthService _authService;

        private const string BaseUrl = "https://example.com";

        [SetUp]
        public void SetUp()
        {
            _httpAdapter = new MockHttpAdapter();
            _tokenStorage = new MockTokenStorage();
            _serializer = new NewtonsoftSerializer();
            _tokenManager = new TokenManager(_serializer, _tokenStorage);
            _pipeline = new HttpPipeline(_httpAdapter, _serializer, _tokenManager);
            _authService = new AuthService(_pipeline, _tokenManager, BaseUrl);
        }

        // =====================================================================
        // Register (Authenticate) — Username
        // =====================================================================

        [Test]
        public async Task AuthenticateUsername_ValidCredentials_ReturnsSession()
        {
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(201, json);

            var session = await _authService.AuthenticateUsernameAsync("player1", "securePass123");

            Assert.AreEqual(TestJwtHelper.DefaultUserId, session.UserId);
        }

        [Test]
        public async Task AuthenticateUsername_ValidCredentials_ReturnsCorrectUsername()
        {
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(201, json);

            var session = await _authService.AuthenticateUsernameAsync("player1", "securePass123");

            Assert.AreEqual(TestJwtHelper.DefaultUsername, session.Username);
        }

        [Test]
        public async Task AuthenticateUsername_ValidCredentials_SendsPostToRegisterEndpoint()
        {
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(201, json);

            await _authService.AuthenticateUsernameAsync("player1", "securePass123");

            Assert.AreEqual(1, _httpAdapter.SentRequests.Count);
            Assert.AreEqual("POST", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/auth/register", _httpAdapter.SentRequests[0].Url);
        }

        [Test]
        public async Task AuthenticateUsername_ValidCredentials_SendsCorrectBody()
        {
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(201, json);

            await _authService.AuthenticateUsernameAsync("player1", "securePass123");

            var body = _serializer.Deserialize<RegisterUsernameRequest>(_httpAdapter.SentRequests[0].Body);
            Assert.AreEqual("username", body.Provider);
            Assert.AreEqual("player1", body.Username);
            Assert.AreEqual("securePass123", body.Password);
        }

        [Test]
        public async Task AuthenticateUsername_ValidCredentials_SavesTokensToStorage()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            var json = TestJwtHelper.BuildAuthResponseJson(accessToken, refreshToken);
            _httpAdapter.EnqueueResponse(201, json);

            await _authService.AuthenticateUsernameAsync("player1", "securePass123");

            Assert.AreEqual(accessToken, _tokenStorage.SavedAuthToken);
            Assert.AreEqual(refreshToken, _tokenStorage.SavedRefreshToken);
        }

        // =====================================================================
        // Register — Email
        // =====================================================================

        [Test]
        public async Task AuthenticateEmail_ValidCredentials_SendsCorrectBody()
        {
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(201, json);

            await _authService.AuthenticateEmailAsync("player@game.com", "securePass123", "player1");

            var body = _serializer.Deserialize<RegisterEmailRequest>(_httpAdapter.SentRequests[0].Body);
            Assert.AreEqual("email", body.Provider);
            Assert.AreEqual("player@game.com", body.Email);
            Assert.AreEqual("securePass123", body.Password);
        }

        // =====================================================================
        // Register — Device
        // =====================================================================

        [Test]
        public async Task AuthenticateDevice_ValidDeviceId_SendsCorrectBody()
        {
            var deviceId = "550e8400-e29b-41d4-a716-446655440000";
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(201, json);

            await _authService.AuthenticateDeviceAsync(deviceId);

            var body = _serializer.Deserialize<RegisterDeviceRequest>(_httpAdapter.SentRequests[0].Body);
            Assert.AreEqual("device", body.Provider);
            Assert.AreEqual(deviceId, body.DeviceId);
        }

        // =====================================================================
        // Login — Username
        // =====================================================================

        [Test]
        public async Task LoginUsername_ValidCredentials_ReturnsSession()
        {
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(200, json);

            var session = await _authService.LoginUsernameAsync("player1", "securePass123");

            Assert.AreEqual(TestJwtHelper.DefaultUserId, session.UserId);
        }

        [Test]
        public async Task LoginUsername_ValidCredentials_SendsPostToLoginEndpoint()
        {
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(200, json);

            await _authService.LoginUsernameAsync("player1", "securePass123");

            Assert.AreEqual("POST", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/auth/login", _httpAdapter.SentRequests[0].Url);
        }

        [Test]
        public async Task LoginUsername_ValidCredentials_SavesTokensToStorage()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            var json = TestJwtHelper.BuildAuthResponseJson(accessToken, refreshToken);
            _httpAdapter.EnqueueResponse(200, json);

            await _authService.LoginUsernameAsync("player1", "securePass123");

            Assert.AreEqual(accessToken, _tokenStorage.SavedAuthToken);
            Assert.AreEqual(refreshToken, _tokenStorage.SavedRefreshToken);
        }

        // =====================================================================
        // Login — Email
        // =====================================================================

        [Test]
        public async Task LoginEmail_ValidCredentials_SendsCorrectBody()
        {
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(200, json);

            await _authService.LoginEmailAsync("player@game.com", "securePass123");

            var body = _serializer.Deserialize<LoginEmailRequest>(_httpAdapter.SentRequests[0].Body);
            Assert.AreEqual("email", body.Provider);
            Assert.AreEqual("player@game.com", body.Email);
            Assert.AreEqual("securePass123", body.Password);
        }

        // =====================================================================
        // Login — Device
        // =====================================================================

        [Test]
        public async Task LoginDevice_ValidDeviceId_SendsPostToLoginEndpoint()
        {
            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(200, json);

            await _authService.LoginDeviceAsync("device-id-123");

            Assert.AreEqual("POST", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/auth/login", _httpAdapter.SentRequests[0].Url);
        }

        // =====================================================================
        // Error handling — Register
        // =====================================================================

        [Test]
        public void AuthenticateUsername_BadRequest400_ThrowsGameApiException()
        {
            _httpAdapter.EnqueueResponse(400, TestJwtHelper.BuildErrorJson("Password too short"));

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _authService.AuthenticateUsernameAsync("p", "short"));

            Assert.AreEqual(400, ex.StatusCode);
        }

        [Test]
        public void AuthenticateUsername_Conflict409_ThrowsGameApiException()
        {
            _httpAdapter.EnqueueResponse(409, TestJwtHelper.BuildErrorJson("Credential already exists"));

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _authService.AuthenticateUsernameAsync("player1", "securePass123"));

            Assert.AreEqual(409, ex.StatusCode);
        }

        // =====================================================================
        // Error handling — Login
        // =====================================================================

        [Test]
        public void LoginUsername_InvalidCredentials401_ThrowsGameApiException()
        {
            _httpAdapter.EnqueueResponse(401, TestJwtHelper.BuildErrorJson("Invalid credentials"));

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _authService.LoginUsernameAsync("player1", "wrongPassword"));

            Assert.AreEqual(401, ex.StatusCode);
        }

        [Test]
        public void LoginUsername_Banned403_ThrowsGameApiException()
        {
            _httpAdapter.EnqueueResponse(403, TestJwtHelper.BuildErrorJson("Account banned"));

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _authService.LoginUsernameAsync("banned_player", "securePass123"));

            Assert.AreEqual(403, ex.StatusCode);
        }

        [Test]
        public void LoginUsername_Banned403_ErrorMessageContainsBanReason()
        {
            _httpAdapter.EnqueueResponse(403, TestJwtHelper.BuildErrorJson("Account banned"));

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _authService.LoginUsernameAsync("banned_player", "securePass123"));

            Assert.AreEqual("Account banned", ex.ErrorMessage);
        }

        // =====================================================================
        // Refresh
        // =====================================================================

        [Test]
        public async Task RefreshSession_ValidRefreshToken_ReturnsNewSession()
        {
            // Pre-set tokens in storage (simulating existing session)
            var oldAccess = TestJwtHelper.CreateValidAccessToken();
            var oldRefresh = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(oldAccess, oldRefresh);
            _tokenManager.LoadFromStorage();

            // Enqueue refresh response with new tokens
            var newAccess = TestJwtHelper.CreateValidAccessToken("new-user-id", "player1", "Player One");
            var newRefresh = TestJwtHelper.CreateValidRefreshToken("new-user-id", "player1", "Player One");
            var json = TestJwtHelper.BuildAuthResponseJson(newAccess, newRefresh, 900, "new-user-id", "player1", "Player One");
            _httpAdapter.EnqueueResponse(200, json);

            var session = await _authService.RefreshSessionAsync();

            Assert.AreEqual(newAccess, session.AuthToken);
        }

        [Test]
        public async Task RefreshSession_ValidRefreshToken_SendsPostToRefreshEndpoint()
        {
            var oldAccess = TestJwtHelper.CreateValidAccessToken();
            var oldRefresh = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(oldAccess, oldRefresh);
            _tokenManager.LoadFromStorage();

            var json = TestJwtHelper.BuildAuthResponseJson();
            _httpAdapter.EnqueueResponse(200, json);

            await _authService.RefreshSessionAsync();

            Assert.AreEqual("POST", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/auth/refresh", _httpAdapter.SentRequests[0].Url);
        }

        // =====================================================================
        // Logout
        // =====================================================================

        [Test]
        public async Task Logout_Authenticated_SendsDeleteToSessionsEndpoint()
        {
            // Set up authenticated session
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();

            _httpAdapter.EnqueueResponse(204, "");

            await _authService.LogoutAsync();

            Assert.AreEqual("DELETE", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/auth/sessions", _httpAdapter.SentRequests[0].Url);
        }

        [Test]
        public async Task Logout_Authenticated_ClearsTokenStorage()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();

            _httpAdapter.EnqueueResponse(204, "");

            await _authService.LogoutAsync();

            Assert.AreEqual(1, _tokenStorage.ClearCallCount);
        }

        [Test]
        public async Task Logout_Authenticated_SendsAuthorizationHeader()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();

            _httpAdapter.EnqueueResponse(204, "");

            await _authService.LogoutAsync();

            Assert.IsTrue(_httpAdapter.SentRequests[0].Headers.ContainsKey("Authorization"));
            Assert.AreEqual($"Bearer {accessToken}", _httpAdapter.SentRequests[0].Headers["Authorization"]);
        }

        // =====================================================================
        // Auto-refresh on 401
        // =====================================================================

        [Test]
        public async Task AutoRefresh_401ThenRefreshSucceeds_RetriesOriginalRequest()
        {
            // Pre-set tokens
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();

            // 1st request → 401 (token expired)
            _httpAdapter.EnqueueResponse(401, TestJwtHelper.BuildErrorJson("Token expired"));

            // 2nd request → refresh succeeds with new tokens
            var newAccess = TestJwtHelper.CreateValidAccessToken("a1b2c3d4-e5f6-7890-abcd-ef1234567890", "player1", "Player One");
            var newRefresh = TestJwtHelper.CreateValidRefreshToken("a1b2c3d4-e5f6-7890-abcd-ef1234567890", "player1", "Player One");
            var refreshJson = TestJwtHelper.BuildAuthResponseJson(newAccess, newRefresh);
            _httpAdapter.EnqueueResponse(200, refreshJson);

            // 3rd request → retry of original request succeeds (list sessions as example)
            _httpAdapter.EnqueueResponse(200, "[]");

            await _authService.ListSessionsAsync();

            // Verify: original request, refresh request, retry of original request
            Assert.AreEqual(3, _httpAdapter.SentRequests.Count);
        }

        [Test]
        public async Task AutoRefresh_401ThenRefreshSucceeds_SecondRequestIsRefresh()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();

            _httpAdapter.EnqueueResponse(401, TestJwtHelper.BuildErrorJson("Token expired"));

            var newAccess = TestJwtHelper.CreateValidAccessToken();
            var newRefresh = TestJwtHelper.CreateValidRefreshToken();
            var refreshJson = TestJwtHelper.BuildAuthResponseJson(newAccess, newRefresh);
            _httpAdapter.EnqueueResponse(200, refreshJson);

            _httpAdapter.EnqueueResponse(200, "[]");

            await _authService.ListSessionsAsync();

            Assert.AreEqual($"{BaseUrl}/api/auth/refresh", _httpAdapter.SentRequests[1].Url);
        }

        [Test]
        public void AutoRefresh_401ThenRefreshFails_ThrowsAndClearsTokens()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();

            // 1st request → 401
            _httpAdapter.EnqueueResponse(401, TestJwtHelper.BuildErrorJson("Token expired"));

            // Refresh also fails → 401
            _httpAdapter.EnqueueResponse(401, TestJwtHelper.BuildErrorJson("Refresh token invalid"));

            Assert.ThrowsAsync<GameApiException>(async () =>
                await _authService.ListSessionsAsync());

            Assert.AreEqual(1, _tokenStorage.ClearCallCount);
        }

        // =====================================================================
        // Session management endpoints
        // =====================================================================

        [Test]
        public async Task ListSessions_Authenticated_SendsGetToSessionsEndpoint()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();

            var sessionsJson = "[{\"id\":\"session-1\",\"device_info\":\"Test\",\"ip_address\":\"127.0.0.1\","
                             + "\"created_at\":\"2026-03-08T12:00:00Z\",\"expires_at\":\"2026-04-07T12:00:00Z\"}]";
            _httpAdapter.EnqueueResponse(200, sessionsJson);

            var sessions = await _authService.ListSessionsAsync();

            Assert.AreEqual("GET", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/auth/sessions", _httpAdapter.SentRequests[0].Url);
        }

        [Test]
        public async Task ListSessions_Authenticated_DeserializesSessionList()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();

            var sessionsJson = "[{\"id\":\"session-1\",\"device_info\":\"Test\",\"ip_address\":\"127.0.0.1\","
                             + "\"created_at\":\"2026-03-08T12:00:00Z\",\"expires_at\":\"2026-04-07T12:00:00Z\"}]";
            _httpAdapter.EnqueueResponse(200, sessionsJson);

            var sessions = await _authService.ListSessionsAsync();

            Assert.AreEqual(1, sessions.Count);
            Assert.AreEqual("session-1", sessions[0].Id);
        }

        [Test]
        public async Task RevokeSession_ValidSessionId_SendsDeleteWithSessionId()
        {
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();

            _httpAdapter.EnqueueResponse(204, "");

            await _authService.RevokeSessionAsync("session-123");

            Assert.AreEqual("DELETE", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/auth/sessions/session-123", _httpAdapter.SentRequests[0].Url);
        }
    }

    // =========================================================================
    // Request model stubs for deserialization in tests.
    // These mirror what AuthService should serialize when sending requests.
    // If CORE agent defines these in AuthModels.cs, remove these and use those.
    // =========================================================================

    internal class RegisterUsernameRequest
    {
        [Newtonsoft.Json.JsonProperty("provider")] public string Provider;
        [Newtonsoft.Json.JsonProperty("username")] public string Username;
        [Newtonsoft.Json.JsonProperty("password")] public string Password;
    }

    internal class RegisterEmailRequest
    {
        [Newtonsoft.Json.JsonProperty("provider")] public string Provider;
        [Newtonsoft.Json.JsonProperty("email")] public string Email;
        [Newtonsoft.Json.JsonProperty("password")] public string Password;
    }

    internal class RegisterDeviceRequest
    {
        [Newtonsoft.Json.JsonProperty("provider")] public string Provider;
        [Newtonsoft.Json.JsonProperty("device_id")] public string DeviceId;
    }

    internal class LoginEmailRequest
    {
        [Newtonsoft.Json.JsonProperty("provider")] public string Provider;
        [Newtonsoft.Json.JsonProperty("email")] public string Email;
        [Newtonsoft.Json.JsonProperty("password")] public string Password;
    }
}
