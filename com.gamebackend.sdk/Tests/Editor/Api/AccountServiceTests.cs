using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameBackend.Api;
using GameBackend.Core.Exceptions;
using GameBackend.Core.Models;
using GameBackend.Tests.Mocks;
using GameBackend.Transport;
using GameBackend.Transport.Interfaces;
using NUnit.Framework;

namespace GameBackend.Tests.Api
{
    /// <summary>
    /// Tests for AccountService via MockHttpAdapter.
    /// These tests define the contract for account operations (TDD Red phase).
    ///
    /// Dependencies that DO NOT exist yet (will be created by REST agent):
    ///   - GameBackend.Api.Services.AccountService : IAccountClient
    ///
    /// All Account endpoints require authorization.
    /// </summary>
    [TestFixture]
    public class AccountServiceTests
    {
        private MockHttpAdapter _httpAdapter;
        private MockTokenStorage _tokenStorage;
        private ISerializer _serializer;
        private TokenManager _tokenManager;
        private HttpPipeline _pipeline;
        private AccountService _accountService;

        private const string BaseUrl = "http://localhost:30080";

        [SetUp]
        public void SetUp()
        {
            _httpAdapter = new MockHttpAdapter();
            _tokenStorage = new MockTokenStorage();
            _serializer = new NewtonsoftSerializer();
            _tokenManager = new TokenManager(_serializer, _tokenStorage);
            _pipeline = new HttpPipeline(_httpAdapter, _serializer, _tokenManager);
            _accountService = new AccountService(_pipeline, BaseUrl);

            // All Account endpoints require auth — set up a valid session
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();
        }

        // =====================================================================
        // GET /api/account/me
        // =====================================================================

        [Test]
        public async Task GetAccount_Success_ReturnsAccount()
        {
            var json = @"{
                ""id"":""user-123"",
                ""username"":""player1"",
                ""display_name"":""Player One"",
                ""avatar_url"":""https://example.com/avatar.png"",
                ""lang"":""en"",
                ""location"":""Moscow"",
                ""timezone"":""Europe/Moscow"",
                ""is_banned"":false,
                ""ban_reason"":null,
                ""created_at"":""2026-02-26T12:00:00Z"",
                ""updated_at"":""2026-02-28T10:00:00Z"",
                ""banned_at"":null,
                ""banned_by"":null
            }";
            _httpAdapter.EnqueueResponse(200, json);

            var account = await _accountService.GetAccountAsync();

            Assert.AreEqual("user-123", account.Id);
            Assert.AreEqual("player1", account.Username);
            Assert.AreEqual("Player One", account.DisplayName);
            Assert.AreEqual("https://example.com/avatar.png", account.AvatarUrl);
            Assert.AreEqual("en", account.Lang);
            Assert.AreEqual("Moscow", account.Location);
            Assert.AreEqual("Europe/Moscow", account.Timezone);
            Assert.IsFalse(account.IsBanned);
        }

        [Test]
        public async Task GetAccount_Success_SendsGetToMeEndpoint()
        {
            var json = @"{""id"":""user-123"",""username"":""player1"",""display_name"":""P1""}";
            _httpAdapter.EnqueueResponse(200, json);

            await _accountService.GetAccountAsync();

            Assert.AreEqual(1, _httpAdapter.SentRequests.Count);
            Assert.AreEqual("GET", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/account/me", _httpAdapter.SentRequests[0].Url);
        }

        // =====================================================================
        // PUT /api/account/me
        // =====================================================================

        [Test]
        public async Task UpdateAccount_DisplayName_SendsPut()
        {
            var json = @"{""id"":""user-123"",""username"":""player1"",""display_name"":""New Name""}";
            _httpAdapter.EnqueueResponse(200, json);

            await _accountService.UpdateAccountAsync(displayName: "New Name");

            Assert.AreEqual(1, _httpAdapter.SentRequests.Count);
            Assert.AreEqual("PUT", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/account/me", _httpAdapter.SentRequests[0].Url);
            Assert.IsTrue(_httpAdapter.SentRequests[0].Body.Contains("display_name"));
        }

        [Test]
        public async Task UpdateAccount_AllFields_SendsAllInBody()
        {
            var json = @"{""id"":""user-123"",""username"":""player1"",""display_name"":""New Name""}";
            _httpAdapter.EnqueueResponse(200, json);

            await _accountService.UpdateAccountAsync(
                displayName: "New Name",
                avatarUrl: "https://example.com/new.png",
                lang: "ru",
                location: "Saint Petersburg",
                timezone: "Europe/Moscow"
            );

            var body = _httpAdapter.SentRequests[0].Body;
            Assert.IsTrue(body.Contains("display_name"), "Body should contain display_name");
            Assert.IsTrue(body.Contains("avatar_url"), "Body should contain avatar_url");
            Assert.IsTrue(body.Contains("lang"), "Body should contain lang");
            Assert.IsTrue(body.Contains("location"), "Body should contain location");
            Assert.IsTrue(body.Contains("timezone"), "Body should contain timezone");
        }

        // =====================================================================
        // DELETE /api/account/me
        // =====================================================================

        [Test]
        public async Task DeleteAccount_Success_SendsDelete()
        {
            _httpAdapter.EnqueueResponse(204, "");

            await _accountService.DeleteAccountAsync();

            Assert.AreEqual(1, _httpAdapter.SentRequests.Count);
            Assert.AreEqual("DELETE", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/account/me", _httpAdapter.SentRequests[0].Url);
        }

        // =====================================================================
        // GET /api/account/{id}
        // =====================================================================

        [Test]
        public async Task GetUser_ValidId_ReturnsUser()
        {
            var json = @"{""id"":""some-user-id"",""username"":""player2"",""display_name"":""Player Two"",""avatar_url"":null}";
            _httpAdapter.EnqueueResponse(200, json);

            var user = await _accountService.GetUserAsync("some-user-id");

            Assert.AreEqual("some-user-id", user.Id);
            Assert.AreEqual("player2", user.Username);
            Assert.AreEqual("Player Two", user.DisplayName);
            Assert.IsNull(user.AvatarUrl);
            Assert.AreEqual("GET", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/account/some-user-id", _httpAdapter.SentRequests[0].Url);
        }

        // =====================================================================
        // POST /api/account/batch
        // =====================================================================

        [Test]
        public async Task GetUsers_BatchIds_ReturnsUsers()
        {
            var json = @"[
                {""id"":""user-1"",""username"":""p1"",""display_name"":""P1"",""avatar_url"":null},
                {""id"":""user-2"",""username"":""p2"",""display_name"":""P2"",""avatar_url"":null}
            ]";
            _httpAdapter.EnqueueResponse(200, json);

            var users = await _accountService.GetUsersAsync(new[] { "user-1", "user-2" });
            var userList = users.ToList();

            Assert.AreEqual(2, userList.Count);
            Assert.AreEqual("user-1", userList[0].Id);
            Assert.AreEqual("user-2", userList[1].Id);
            Assert.AreEqual("POST", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/account/batch", _httpAdapter.SentRequests[0].Url);
            Assert.IsTrue(_httpAdapter.SentRequests[0].Body.Contains("user_ids"));
        }

        // =====================================================================
        // Error handling
        // =====================================================================

        [Test]
        public void GetAccount_401_ThrowsGameApiException()
        {
            // First response: 401 triggers auto-refresh
            _httpAdapter.EnqueueResponse(401, @"{""error"":""Unauthorized""}");
            // Second response: refresh also fails with 401
            _httpAdapter.EnqueueResponse(401, @"{""error"":""Unauthorized""}");

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _accountService.GetAccountAsync());

            Assert.AreEqual(401, ex.StatusCode);
        }

        [Test]
        public void GetAccount_404_ThrowsGameApiException()
        {
            _httpAdapter.EnqueueResponse(404, @"{""error"":""User not found""}");

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _accountService.GetAccountAsync());

            Assert.AreEqual(404, ex.StatusCode);
            Assert.AreEqual("User not found", ex.ErrorMessage);
        }
    }

    // =========================================================================
    // Request model stubs for deserialization in tests.
    // These mirror what AccountService should serialize when sending requests.
    // =========================================================================

    internal class UpdateAccountRequest
    {
        [Newtonsoft.Json.JsonProperty("display_name")] public string DisplayName;
        [Newtonsoft.Json.JsonProperty("avatar_url")] public string AvatarUrl;
        [Newtonsoft.Json.JsonProperty("lang")] public string Lang;
        [Newtonsoft.Json.JsonProperty("location")] public string Location;
        [Newtonsoft.Json.JsonProperty("timezone")] public string Timezone;
    }

    internal class BatchUserIdsRequest
    {
        [Newtonsoft.Json.JsonProperty("user_ids")] public List<string> UserIds;
    }
}
