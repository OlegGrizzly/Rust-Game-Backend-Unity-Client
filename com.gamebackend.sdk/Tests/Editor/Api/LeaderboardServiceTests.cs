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
    /// Tests for LeaderboardService via MockHttpAdapter.
    /// These tests define the contract for leaderboard operations (TDD Red phase).
    ///
    /// Dependencies that DO NOT exist yet (will be created by REST agent):
    ///   - GameBackend.Api.Services.LeaderboardService : ILeaderboardClient
    ///
    /// All Leaderboard endpoints require authorization.
    /// </summary>
    [TestFixture]
    public class LeaderboardServiceTests
    {
        private MockHttpAdapter _httpAdapter;
        private MockTokenStorage _tokenStorage;
        private ISerializer _serializer;
        private TokenManager _tokenManager;
        private HttpPipeline _pipeline;
        private LeaderboardService _leaderboardService;

        private const string BaseUrl = "http://localhost:30080";

        [SetUp]
        public void SetUp()
        {
            _httpAdapter = new MockHttpAdapter();
            _tokenStorage = new MockTokenStorage();
            _serializer = new NewtonsoftSerializer();
            _tokenManager = new TokenManager(_serializer, _tokenStorage);
            _pipeline = new HttpPipeline(_httpAdapter, _serializer, _tokenManager);
            _leaderboardService = new LeaderboardService(_pipeline, BaseUrl);

            // All Leaderboard endpoints require auth — set up a valid session
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();
        }

        // =====================================================================
        // POST /api/leaderboard/leaderboards/{id}/record
        // =====================================================================

        [Test]
        public async Task WriteRecord_Score_SendsPost()
        {
            var json = @"{
                ""leaderboard_id"":""weekly_kills"",
                ""user_id"":""user-123"",
                ""score"":1500,
                ""subscore"":0,
                ""metadata"":null,
                ""rank"":0,
                ""created_at"":""2026-02-28T12:00:00Z"",
                ""updated_at"":""2026-02-28T14:30:00Z""
            }";
            _httpAdapter.EnqueueResponse(200, json);

            await _leaderboardService.WriteLeaderboardRecordAsync("weekly_kills", 1500);

            Assert.AreEqual(1, _httpAdapter.SentRequests.Count);
            Assert.AreEqual("POST", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/leaderboard/leaderboards/weekly_kills/record",
                _httpAdapter.SentRequests[0].Url);
            Assert.IsTrue(_httpAdapter.SentRequests[0].Body.Contains("1500"));
        }

        [Test]
        public async Task WriteRecord_WithSubscoreAndMetadata_SendsAll()
        {
            var json = @"{
                ""leaderboard_id"":""weekly_kills"",
                ""user_id"":""user-123"",
                ""score"":1500,
                ""subscore"":42,
                ""metadata"":{""level"":10},
                ""rank"":0,
                ""created_at"":""2026-02-28T12:00:00Z"",
                ""updated_at"":""2026-02-28T14:30:00Z""
            }";
            _httpAdapter.EnqueueResponse(200, json);

            await _leaderboardService.WriteLeaderboardRecordAsync("weekly_kills", 1500,
                subscore: 42, metadata: new Dictionary<string, object> { { "level", 10L } });

            var body = _httpAdapter.SentRequests[0].Body;
            Assert.IsTrue(body.Contains("\"score\""), "Body should contain score");
            Assert.IsTrue(body.Contains("\"subscore\""), "Body should contain subscore");
            Assert.IsTrue(body.Contains("\"metadata\""), "Body should contain metadata");
        }

        // =====================================================================
        // GET /api/leaderboard/leaderboards/{id}?limit=N
        // =====================================================================

        [Test]
        public async Task ListRecords_Top10_ReturnsRecordList()
        {
            var json = @"[
                {""leaderboard_id"":""weekly_kills"",""user_id"":""user-1"",""score"":2500,""subscore"":100,""rank"":1,""metadata"":null,""created_at"":""2026-02-28T10:00:00Z"",""updated_at"":""2026-02-28T14:30:00Z""},
                {""leaderboard_id"":""weekly_kills"",""user_id"":""user-2"",""score"":2100,""subscore"":50,""rank"":2,""metadata"":null,""created_at"":""2026-02-28T11:00:00Z"",""updated_at"":""2026-02-28T13:00:00Z""}
            ]";
            _httpAdapter.EnqueueResponse(200, json);

            var result = await _leaderboardService.ListLeaderboardRecordsAsync("weekly_kills", limit: 10);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Records);
            var records = result.Records.ToList();
            Assert.AreEqual(2, records.Count);
            Assert.AreEqual(1, records[0].Rank);
            Assert.AreEqual(2500, records[0].Score);
            Assert.AreEqual("GET", _httpAdapter.SentRequests[0].Method);
            Assert.IsTrue(_httpAdapter.SentRequests[0].Url.Contains("?limit=10"));
        }

        // =====================================================================
        // GET /api/leaderboard/leaderboards/{id}/around/{user_id}
        // =====================================================================

        [Test]
        public async Task ListRecordsAroundUser_ReturnsRecordList()
        {
            var json = @"[
                {""leaderboard_id"":""weekly_kills"",""user_id"":""user-123"",""score"":1800,""subscore"":0,""rank"":5,""metadata"":null,""created_at"":""2026-02-28T10:00:00Z"",""updated_at"":""2026-02-28T14:30:00Z""}
            ]";
            _httpAdapter.EnqueueResponse(200, json);

            var result = await _leaderboardService.ListLeaderboardRecordsAroundUserAsync(
                "weekly_kills", "user-123");

            Assert.IsNotNull(result);
            Assert.AreEqual("GET", _httpAdapter.SentRequests[0].Method);
            Assert.IsTrue(_httpAdapter.SentRequests[0].Url.Contains("/around/user-123"));
        }

        // =====================================================================
        // DELETE /api/leaderboard/leaderboards/{id}/record
        // =====================================================================

        [Test]
        public async Task DeleteRecord_SendsDelete()
        {
            _httpAdapter.EnqueueResponse(204, "");

            await _leaderboardService.DeleteLeaderboardRecordAsync("weekly_kills");

            Assert.AreEqual(1, _httpAdapter.SentRequests.Count);
            Assert.AreEqual("DELETE", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/leaderboard/leaderboards/weekly_kills/record",
                _httpAdapter.SentRequests[0].Url);
        }

        // =====================================================================
        // POST /api/leaderboard/leaderboards/{id}/records/batch
        // =====================================================================

        [Test]
        public async Task GetRecordsByIds_SendsPost()
        {
            var json = @"[
                {""leaderboard_id"":""weekly_kills"",""user_id"":""user-1"",""score"":2500,""subscore"":100,""rank"":1,""metadata"":null,""created_at"":""2026-02-28T10:00:00Z"",""updated_at"":""2026-02-28T14:30:00Z""},
                {""leaderboard_id"":""weekly_kills"",""user_id"":""user-2"",""score"":2100,""subscore"":50,""rank"":2,""metadata"":null,""created_at"":""2026-02-28T11:00:00Z"",""updated_at"":""2026-02-28T13:00:00Z""}
            ]";
            _httpAdapter.EnqueueResponse(200, json);

            var records = await _leaderboardService.GetLeaderboardRecordsByIdsAsync(
                "weekly_kills", new[] { "user-1", "user-2" });
            var recordList = records.ToList();

            Assert.AreEqual(2, recordList.Count);
            Assert.AreEqual("POST", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/leaderboard/leaderboards/weekly_kills/records/batch",
                _httpAdapter.SentRequests[0].Url);
            Assert.IsTrue(_httpAdapter.SentRequests[0].Body.Contains("user_ids"));
        }

        // =====================================================================
        // Error handling
        // =====================================================================

        [Test]
        public void WriteRecord_404_ThrowsGameApiException()
        {
            _httpAdapter.EnqueueResponse(404, @"{""error"":""Leaderboard not found""}");

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _leaderboardService.WriteLeaderboardRecordAsync("nonexistent", 100));

            Assert.AreEqual(404, ex.StatusCode);
            Assert.AreEqual("Leaderboard not found", ex.ErrorMessage);
        }

        [Test]
        public void ListRecords_401_ThrowsGameApiException()
        {
            // First response: 401 triggers auto-refresh
            _httpAdapter.EnqueueResponse(401, @"{""error"":""Unauthorized""}");
            // Second response: refresh also fails with 401
            _httpAdapter.EnqueueResponse(401, @"{""error"":""Unauthorized""}");

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _leaderboardService.ListLeaderboardRecordsAsync("weekly_kills"));

            Assert.AreEqual(401, ex.StatusCode);
        }
    }

    // =========================================================================
    // Request model stubs for deserialization in tests.
    // =========================================================================

    internal class WriteRecordRequest
    {
        [Newtonsoft.Json.JsonProperty("score")] public long Score;
        [Newtonsoft.Json.JsonProperty("subscore")] public long Subscore;
        [Newtonsoft.Json.JsonProperty("metadata")] public Dictionary<string, object> Metadata;
    }

    internal class BatchRecordIdsRequest
    {
        [Newtonsoft.Json.JsonProperty("user_ids")] public List<string> UserIds;
    }
}
