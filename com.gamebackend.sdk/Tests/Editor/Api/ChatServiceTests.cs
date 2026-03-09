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
    /// Tests for ChatService via MockHttpAdapter.
    /// These tests define the contract for chat/channel REST operations (TDD Red phase).
    ///
    /// Dependencies that DO NOT exist yet (will be created by REST agent):
    ///   - GameBackend.Api.Services.ChatService : IChatClient
    ///
    /// All Chat endpoints require authorization.
    /// </summary>
    [TestFixture]
    public class ChatServiceTests
    {
        private MockHttpAdapter _httpAdapter;
        private MockTokenStorage _tokenStorage;
        private ISerializer _serializer;
        private TokenManager _tokenManager;
        private HttpPipeline _pipeline;
        private ChatService _chatService;

        private const string BaseUrl = "http://localhost:30080";

        [SetUp]
        public void SetUp()
        {
            _httpAdapter = new MockHttpAdapter();
            _tokenStorage = new MockTokenStorage();
            _serializer = new NewtonsoftSerializer();
            _tokenManager = new TokenManager(_serializer, _tokenStorage);
            _pipeline = new HttpPipeline(_httpAdapter, _serializer, _tokenManager);
            _chatService = new ChatService(_pipeline, BaseUrl);

            // All Chat endpoints require auth — set up a valid session
            var accessToken = TestJwtHelper.CreateValidAccessToken();
            var refreshToken = TestJwtHelper.CreateValidRefreshToken();
            _tokenStorage.Save(accessToken, refreshToken);
            _tokenManager.LoadFromStorage();
        }

        // =====================================================================
        // GET /api/ws/channels
        // =====================================================================

        [Test]
        public async Task ListChannels_ReturnsChannels()
        {
            var json = @"[
                {""id"":""ch-1"",""name"":""general"",""channel_type"":""group"",""context"":null,""created_by"":""user-1"",""created_at"":""2026-02-28T12:00:00Z""},
                {""id"":""ch-2"",""name"":""lobby"",""channel_type"":""group"",""context"":null,""created_by"":""user-2"",""created_at"":""2026-02-28T13:00:00Z""}
            ]";
            _httpAdapter.EnqueueResponse(200, json);

            var channels = await _chatService.ListChannelsAsync();
            var channelList = channels.ToList();

            Assert.AreEqual(2, channelList.Count);
            Assert.AreEqual("ch-1", channelList[0].Id);
            Assert.AreEqual("general", channelList[0].Name);
            Assert.AreEqual("ch-2", channelList[1].Id);
            Assert.AreEqual("GET", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/ws/channels", _httpAdapter.SentRequests[0].Url);
        }

        // =====================================================================
        // POST /api/ws/channels
        // =====================================================================

        [Test]
        public async Task CreateChannel_SendsRequest_ReturnsChannel()
        {
            var json = @"{
                ""id"":""ch-new"",
                ""name"":""my-channel"",
                ""channel_type"":""direct"",
                ""context"":null,
                ""created_by"":""user-1"",
                ""created_at"":""2026-03-01T10:00:00Z""
            }";
            _httpAdapter.EnqueueResponse(200, json);

            var request = new CreateChannelRequest
            {
                Name = "my-channel",
                ChannelType = "direct",
                MemberIds = new[] { "user-1", "user-2" }
            };

            var channel = await _chatService.CreateChannelAsync(request);

            Assert.AreEqual("ch-new", channel.Id);
            Assert.AreEqual("my-channel", channel.Name);
            Assert.AreEqual("direct", channel.ChannelType);
            Assert.AreEqual("POST", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/ws/channels", _httpAdapter.SentRequests[0].Url);
            Assert.IsTrue(_httpAdapter.SentRequests[0].Body.Contains("my-channel"),
                "Body should contain channel name");
        }

        // =====================================================================
        // GET /api/ws/channels/{id}/messages?limit=N&cursor=X
        // =====================================================================

        [Test]
        public async Task ListMessages_WithLimitAndCursor()
        {
            var json = @"{
                ""messages"":[
                    {""id"":""msg-1"",""channel_id"":""ch-1"",""sender_id"":""user-1"",""sender_name"":""Player1"",""content"":""Hello"",""created_at"":""2026-03-01T12:00:00Z"",""deleted_at"":null},
                    {""id"":""msg-2"",""channel_id"":""ch-1"",""sender_id"":""user-2"",""sender_name"":""Player2"",""content"":""Hi"",""created_at"":""2026-03-01T12:01:00Z"",""deleted_at"":null}
                ],
                ""next_cursor"":""cursor-xyz"",
                ""prev_cursor"":""cursor-prev"",
                ""has_more"":true
            }";
            _httpAdapter.EnqueueResponse(200, json);

            var result = await _chatService.ListMessagesAsync("ch-1", limit: 20, cursor: "cursor-abc");

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Messages.Count);
            Assert.AreEqual("msg-1", result.Messages[0].Id);
            Assert.AreEqual("cursor-xyz", result.NextCursor);
            Assert.AreEqual("cursor-prev", result.PrevCursor);
            Assert.IsTrue(result.HasMore);
            Assert.AreEqual("GET", _httpAdapter.SentRequests[0].Method);
            Assert.IsTrue(_httpAdapter.SentRequests[0].Url.Contains("/api/ws/channels/ch-1/messages"),
                $"Expected messages URL, got: {_httpAdapter.SentRequests[0].Url}");
            Assert.IsTrue(_httpAdapter.SentRequests[0].Url.Contains("limit=20"),
                $"Expected limit param, got: {_httpAdapter.SentRequests[0].Url}");
            Assert.IsTrue(_httpAdapter.SentRequests[0].Url.Contains("cursor=cursor-abc"),
                $"Expected cursor param, got: {_httpAdapter.SentRequests[0].Url}");
        }

        // =====================================================================
        // GET /api/ws/channels/{id}/unread
        // =====================================================================

        [Test]
        public async Task GetUnread_ReturnsUnreadInfo()
        {
            var json = @"{""unread_count"":5}";
            _httpAdapter.EnqueueResponse(200, json);

            var unread = await _chatService.GetUnreadAsync("ch-1");

            Assert.AreEqual(5, unread.UnreadCount);
            Assert.AreEqual("GET", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/ws/channels/ch-1/unread", _httpAdapter.SentRequests[0].Url);
        }

        // =====================================================================
        // POST /api/ws/channels/{id}/read
        // =====================================================================

        [Test]
        public async Task MarkChannelRead_SendsPost()
        {
            _httpAdapter.EnqueueResponse(204, "");

            await _chatService.MarkChannelReadAsync("ch-1");

            Assert.AreEqual(1, _httpAdapter.SentRequests.Count);
            Assert.AreEqual("POST", _httpAdapter.SentRequests[0].Method);
            Assert.AreEqual($"{BaseUrl}/api/ws/channels/ch-1/read", _httpAdapter.SentRequests[0].Url);
        }

        // =====================================================================
        // Error handling
        // =====================================================================

        [Test]
        public void ListChannels_401_ThrowsAfterRefreshFails()
        {
            // First response: 401 triggers auto-refresh
            _httpAdapter.EnqueueResponse(401, @"{""error"":""Unauthorized""}");
            // Second response: refresh also fails with 401
            _httpAdapter.EnqueueResponse(401, @"{""error"":""Unauthorized""}");

            var ex = Assert.ThrowsAsync<GameApiException>(async () =>
                await _chatService.ListChannelsAsync());

            Assert.AreEqual(401, ex.StatusCode);
        }
    }
}
