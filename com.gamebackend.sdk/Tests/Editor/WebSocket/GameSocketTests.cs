using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;
using GameBackend.Tests.Mocks;
using GameBackend.Transport;
using GameBackend.Transport.Interfaces;
using GameBackend.WebSocket;
using NUnit.Framework;

namespace GameBackend.Tests.WebSocket
{
    /// <summary>
    /// Tests for GameSocket via MockWebSocketAdapter.
    /// These tests define the contract for WebSocket operations (TDD Red phase).
    ///
    /// Dependencies that DO NOT exist yet (will be created by WS agent):
    ///   - GameBackend.WebSocket.GameSocket : IGameSocket
    ///   - GameBackend.WebSocket.GameSocketOptions
    ///
    /// GameSocket wraps IWebSocketAdapter, dispatches incoming messages to typed events,
    /// sends heartbeat pings, and handles reconnection on unexpected close.
    /// </summary>
    [TestFixture]
    public class GameSocketTests
    {
        private MockWebSocketAdapter _wsAdapter;
        private ISerializer _serializer;
        private GameSocket _gameSocket;

        private const string Host = "localhost:30080";
        private const string AccessToken = "test-access-token";

        [SetUp]
        public void SetUp()
        {
            _wsAdapter = new MockWebSocketAdapter();
            _serializer = new NewtonsoftSerializer();
            _gameSocket = new GameSocket(_wsAdapter, _serializer, new GameSocketOptions
            {
                Host = Host,
                AccessToken = AccessToken,
                HeartbeatInterval = 30f,
                PongTimeout = 10f,
                ReconnectEnabled = false // disable reconnect for most tests
            });
        }

        [TearDown]
        public void TearDown()
        {
            _gameSocket?.Dispose();
        }

        // =====================================================================
        // Connect
        // =====================================================================

        [Test]
        public async Task Connect_SetsUrl_WithToken()
        {
            await _gameSocket.ConnectAsync();

            Assert.IsNotNull(_wsAdapter.LastConnectUrl);
            Assert.IsTrue(
                _wsAdapter.LastConnectUrl.Contains($"ws://{Host}/ws?token={AccessToken}"),
                $"Expected URL to contain ws://{Host}/ws?token={AccessToken}, got: {_wsAdapter.LastConnectUrl}");
        }

        // =====================================================================
        // Close
        // =====================================================================

        [Test]
        public async Task Close_DisconnectsAdapter()
        {
            await _gameSocket.ConnectAsync();
            await _gameSocket.CloseAsync();

            Assert.IsFalse(_wsAdapter.IsConnected);
        }

        // =====================================================================
        // Send
        // =====================================================================

        [Test]
        public async Task SendChatMessage_SerializesCorrectly()
        {
            await _gameSocket.ConnectAsync();

            await _gameSocket.SendChatMessageAsync("channel-abc", "Hello world");

            Assert.AreEqual(1, _wsAdapter.SentMessages.Count);
            var sent = _wsAdapter.SentMessages[0];
            Assert.IsTrue(sent.Contains("\"type\":\"chat.send\"") || sent.Contains("\"type\": \"chat.send\""),
                $"Expected type 'chat.send' in: {sent}");
            Assert.IsTrue(sent.Contains("channel_id") || sent.Contains("channel-abc"),
                $"Expected channel_id in: {sent}");
            Assert.IsTrue(sent.Contains("Hello world"),
                $"Expected content in: {sent}");
        }

        // =====================================================================
        // Incoming messages — event dispatching
        // =====================================================================

        [Test]
        public async Task OnMessage_ChatMessage_FiresEvent()
        {
            await _gameSocket.ConnectAsync();

            ChatMessage received = null;
            _gameSocket.ReceivedChatMessage += msg => received = msg;

            var json = @"{
                ""type"":""chat.message"",
                ""data"":{
                    ""id"":""msg-1"",
                    ""channel_id"":""ch-1"",
                    ""sender_id"":""user-1"",
                    ""sender_name"":""Player1"",
                    ""content"":""Hello!"",
                    ""created_at"":""2026-03-01T12:00:00Z"",
                    ""deleted_at"":null
                }
            }";
            _wsAdapter.SimulateMessage(json);

            Assert.IsNotNull(received, "ReceivedChatMessage event should have fired");
            Assert.AreEqual("msg-1", received.Id);
            Assert.AreEqual("ch-1", received.ChannelId);
            Assert.AreEqual("user-1", received.SenderId);
            Assert.AreEqual("Hello!", received.Content);
        }

        [Test]
        public async Task OnMessage_PresenceUpdate_FiresEvent()
        {
            await _gameSocket.ConnectAsync();

            PresenceUpdate received = null;
            _gameSocket.ReceivedPresenceUpdate += p => received = p;

            var json = @"{
                ""type"":""presence.update"",
                ""data"":{
                    ""user_id"":""user-42"",
                    ""username"":""player42"",
                    ""online"":true,
                    ""status"":""in_game""
                }
            }";
            _wsAdapter.SimulateMessage(json);

            Assert.IsNotNull(received, "ReceivedPresenceUpdate event should have fired");
            Assert.AreEqual("user-42", received.UserId);
            Assert.IsTrue(received.Online);
        }

        [Test]
        public async Task OnMessage_SessionRevoked_FiresEvent()
        {
            await _gameSocket.ConnectAsync();

            SessionRevokedEvent received = null;
            _gameSocket.ReceivedSessionRevoked += e => received = e;

            var json = @"{
                ""type"":""session_revoked"",
                ""data"":{
                    ""session_id"":""sess-99"",
                    ""reason"":""logged_in_elsewhere""
                }
            }";
            _wsAdapter.SimulateMessage(json);

            Assert.IsNotNull(received, "ReceivedSessionRevoked event should have fired");
            Assert.AreEqual("sess-99", received.SessionId);
            Assert.AreEqual("logged_in_elsewhere", received.Reason);
        }

        [Test]
        public async Task OnMessage_UserBanned_FiresEvent()
        {
            await _gameSocket.ConnectAsync();

            UserBannedEvent received = null;
            _gameSocket.ReceivedUserBanned += e => received = e;

            var json = @"{
                ""type"":""user_banned"",
                ""data"":{
                    ""reason"":""cheating"",
                    ""expires_at"":""2026-04-01T00:00:00Z""
                }
            }";
            _wsAdapter.SimulateMessage(json);

            Assert.IsNotNull(received, "ReceivedUserBanned event should have fired");
            Assert.AreEqual("cheating", received.Reason);
        }

        [Test]
        public async Task OnMessage_Notification_FiresEvent()
        {
            await _gameSocket.ConnectAsync();

            NotificationEvent received = null;
            _gameSocket.ReceivedNotification += e => received = e;

            var json = @"{
                ""type"":""notification"",
                ""data"":{
                    ""id"":""notif-1"",
                    ""type"":""reward"",
                    ""content"":""You earned 100 coins"",
                    ""created_at"":""2026-03-01T15:00:00Z""
                }
            }";
            _wsAdapter.SimulateMessage(json);

            Assert.IsNotNull(received, "ReceivedNotification event should have fired");
            Assert.AreEqual("notif-1", received.Id);
            Assert.AreEqual("reward", received.Type);
        }

        [Test]
        public async Task OnMessage_UnknownType_DoesNotThrow()
        {
            await _gameSocket.ConnectAsync();

            var json = @"{""type"":""some.future.event"",""data"":{""foo"":""bar""}}";

            Assert.DoesNotThrow(() => _wsAdapter.SimulateMessage(json));
        }

        // =====================================================================
        // Reconnect
        // =====================================================================

        [Test]
        public async Task OnClose_TriggersReconnect()
        {
            // Use a dedicated adapter to avoid interference from _gameSocket
            var dedicatedAdapter = new MockWebSocketAdapter();
            var reconnectSocket = new GameSocket(dedicatedAdapter, _serializer, new GameSocketOptions
            {
                Host = Host,
                AccessToken = AccessToken,
                HeartbeatInterval = 30f,
                PongTimeout = 10f,
                ReconnectEnabled = true
            });

            try
            {
                await reconnectSocket.ConnectAsync();

                // Simulate unexpected close
                dedicatedAdapter.SimulateClose();

                // After unexpected close, the adapter should be asked to reconnect.
                // Give a short delay for the reconnect to trigger.
                await Task.Delay(1500);

                // The reconnect should have attempted to connect again — check URL was set again
                Assert.IsTrue(
                    dedicatedAdapter.LastConnectUrl != null &&
                    dedicatedAdapter.LastConnectUrl.Contains($"ws://{Host}/ws?token="),
                    "Expected reconnect to attempt ConnectAsync with the correct URL");
            }
            finally
            {
                reconnectSocket.Dispose();
            }
        }

        // =====================================================================
        // Heartbeat
        // =====================================================================

        [Test]
        public async Task Heartbeat_SendsPing()
        {
            // Use a dedicated adapter to avoid interference from _gameSocket
            var dedicatedAdapter = new MockWebSocketAdapter();
            var heartbeatSocket = new GameSocket(dedicatedAdapter, _serializer, new GameSocketOptions
            {
                Host = Host,
                AccessToken = AccessToken,
                HeartbeatInterval = 0.1f, // 100ms for fast test
                PongTimeout = 10f,
                ReconnectEnabled = false
            });

            try
            {
                await heartbeatSocket.ConnectAsync();

                // Wait enough for at least one heartbeat
                await Task.Delay(300);

                var pings = dedicatedAdapter.SentMessages
                    .Where(m => m.Contains("\"type\":\"ping\"") || m.Contains("\"type\": \"ping\""))
                    .ToList();

                Assert.IsTrue(pings.Count > 0,
                    $"Expected at least one ping message, got {dedicatedAdapter.SentMessages.Count} messages total");
            }
            finally
            {
                heartbeatSocket.Dispose();
            }
        }

        [Test]
        public async Task Heartbeat_PongTimeout_Disconnects()
        {
            // Use a dedicated adapter to avoid interference from _gameSocket subscribed to _wsAdapter
            var dedicatedAdapter = new MockWebSocketAdapter();
            var timeoutSocket = new GameSocket(dedicatedAdapter, _serializer, new GameSocketOptions
            {
                Host = Host,
                AccessToken = AccessToken,
                HeartbeatInterval = 0.1f, // 100ms
                PongTimeout = 0.2f,       // 200ms — no pong will come
                ReconnectEnabled = false
            });

            try
            {
                await timeoutSocket.ConnectAsync();

                // Use UniTask.Delay to advance PlayerLoop (UniTask.Delay inside GameSocket needs it)
                // In EditMode with many tests, PlayerLoop ticks slower — use generous timeout
                await UniTask.Delay(5000);

                Assert.IsFalse(timeoutSocket.IsConnected,
                    "Socket should disconnect when pong is not received within timeout");
            }
            finally
            {
                timeoutSocket.Dispose();
            }
        }
    }
}
