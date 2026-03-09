using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Interfaces;
using GameBackend.Core.Models;
using GameBackend.Transport.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameBackend.WebSocket
{
    public class GameSocket : IGameSocket
    {
        private readonly IWebSocketAdapter _adapter;
        private readonly ISerializer _serializer;
        private readonly GameSocketOptions _options;
        private readonly ReconnectHandler _reconnectHandler;

        private CancellationTokenSource _heartbeatCts;
        private CancellationTokenSource _pongTimeoutCts;
        private bool _disposed;
        private bool _closedByUser;
        private bool _waitingForPong;

        public GameSocket(IWebSocketAdapter adapter, ISerializer serializer, GameSocketOptions options = null)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _options = options ?? new GameSocketOptions();
            _reconnectHandler = new ReconnectHandler();
            _reconnectHandler.Enabled = _options.ReconnectEnabled;

            _adapter.OnMessage += HandleMessage;
            _adapter.OnClose += HandleClose;
            _adapter.OnError += HandleError;
            _adapter.OnOpen += HandleOpen;
        }

        // === State ===
        public bool IsConnected => _adapter.IsConnected;

        // === Events ===
        public event Action Connected;
        public event Action Closed;
        public event Action<Exception> ReceivedError;
        public event Action<ChatMessage> ReceivedChatMessage;
        public event Action<PresenceUpdate> ReceivedPresenceUpdate;
        public event Action<PresenceStateEvent> ReceivedPresenceState;
        public event Action<ChannelEvent> ReceivedChannelJoined;
        public event Action<ChannelEvent> ReceivedChannelLeft;
        public event Action<ChannelMembersEvent> ReceivedChannelMembers;
        public event Action<ChannelEvent> ReceivedChannelMemberJoined;
        public event Action<ChannelEvent> ReceivedChannelMemberLeft;
        public event Action<SessionRevokedEvent> ReceivedSessionRevoked;
        public event Action<UserBannedEvent> ReceivedUserBanned;
        public event Action<NotificationEvent> ReceivedNotification;
        public event Action<FriendEvent> ReceivedFriendRequest;
        public event Action<FriendEvent> ReceivedFriendAccepted;
        public event Action<GroupEvent> ReceivedGroupJoined;
        public event Action<GroupEvent> ReceivedGroupKicked;
        public event Action<TournamentEvent> ReceivedTournamentStarted;
        public event Action<TournamentEvent> ReceivedTournamentEnded;

        // === Methods ===

        public async UniTask ConnectAsync(CancellationToken ct = default)
        {
            _closedByUser = false;
            var url = $"ws://{_options.Host}/ws?token={_options.AccessToken}";
            await _adapter.ConnectAsync(url, ct);
            _reconnectHandler.Reset();
            StartHeartbeat();
        }

        public async UniTask CloseAsync()
        {
            _closedByUser = true;
            StopHeartbeat();
            await _adapter.CloseAsync();
        }

        public async UniTask SendChatMessageAsync(string channelId, string content, CancellationToken ct = default)
        {
            var envelope = new
            {
                type = "chat.send",
                data = new
                {
                    channel_id = channelId,
                    content = content
                }
            };
            var json = _serializer.Serialize(envelope);
            await _adapter.SendAsync(json, ct);
        }

        // === Private ===

        private void HandleOpen()
        {
            Connected?.Invoke();
        }

        private void HandleMessage(string json)
        {
            WebSocketEnvelope envelope;
            try
            {
                envelope = JsonConvert.DeserializeObject<WebSocketEnvelope>(json);
            }
            catch
            {
                // Malformed JSON -- ignore
                return;
            }

            if (envelope == null || string.IsNullOrEmpty(envelope.Type))
                return;

            switch (envelope.Type)
            {
                case "pong":
                    OnPongReceived();
                    break;
                case "chat.message":
                    ReceivedChatMessage?.Invoke(envelope.Data?.ToObject<ChatMessage>());
                    break;
                case "presence.update":
                    ReceivedPresenceUpdate?.Invoke(envelope.Data?.ToObject<PresenceUpdate>());
                    break;
                case "presence.state":
                    ReceivedPresenceState?.Invoke(envelope.Data?.ToObject<PresenceStateEvent>());
                    break;
                case "session_revoked":
                    ReceivedSessionRevoked?.Invoke(envelope.Data?.ToObject<SessionRevokedEvent>());
                    break;
                case "user_banned":
                    ReceivedUserBanned?.Invoke(envelope.Data?.ToObject<UserBannedEvent>());
                    break;
                case "notification":
                    ReceivedNotification?.Invoke(envelope.Data?.ToObject<NotificationEvent>());
                    break;
                case "channel.joined":
                    ReceivedChannelJoined?.Invoke(envelope.Data?.ToObject<ChannelEvent>());
                    break;
                case "channel.left":
                    ReceivedChannelLeft?.Invoke(envelope.Data?.ToObject<ChannelEvent>());
                    break;
                case "channel.members":
                    ReceivedChannelMembers?.Invoke(envelope.Data?.ToObject<ChannelMembersEvent>());
                    break;
                case "channel.member_joined":
                    ReceivedChannelMemberJoined?.Invoke(envelope.Data?.ToObject<ChannelEvent>());
                    break;
                case "channel.member_left":
                    ReceivedChannelMemberLeft?.Invoke(envelope.Data?.ToObject<ChannelEvent>());
                    break;
                case "friend_request":
                    ReceivedFriendRequest?.Invoke(envelope.Data?.ToObject<FriendEvent>());
                    break;
                case "friend_accepted":
                    ReceivedFriendAccepted?.Invoke(envelope.Data?.ToObject<FriendEvent>());
                    break;
                case "group_joined":
                    ReceivedGroupJoined?.Invoke(envelope.Data?.ToObject<GroupEvent>());
                    break;
                case "group_kicked":
                    ReceivedGroupKicked?.Invoke(envelope.Data?.ToObject<GroupEvent>());
                    break;
                case "tournament_started":
                    ReceivedTournamentStarted?.Invoke(envelope.Data?.ToObject<TournamentEvent>());
                    break;
                case "tournament_ended":
                    ReceivedTournamentEnded?.Invoke(envelope.Data?.ToObject<TournamentEvent>());
                    break;
                case "error":
                    var errorMsg = envelope.Data?.ToString() ?? "Unknown WebSocket error";
                    ReceivedError?.Invoke(new Exception(errorMsg));
                    break;
                default:
                    // Unknown type -- ignore for forward compatibility
                    break;
            }
        }

        private void HandleClose()
        {
            StopHeartbeat();
            Closed?.Invoke();

            if (!_closedByUser && _reconnectHandler.ShouldReconnect)
            {
                AttemptReconnectAsync().Forget();
            }
        }

        private void HandleError(string error)
        {
            ReceivedError?.Invoke(new Exception(error));
        }

        private async UniTaskVoid AttemptReconnectAsync()
        {
            var delay = _reconnectHandler.NextDelay();
            await UniTask.Delay(TimeSpan.FromSeconds(delay));

            if (_disposed || _closedByUser)
                return;

            try
            {
                await ConnectAsync();
            }
            catch
            {
                // Reconnect failed -- HandleClose will be called again by the adapter,
                // which will trigger another reconnect attempt with increased backoff.
            }
        }

        // === Heartbeat ===

        private void StartHeartbeat()
        {
            StopHeartbeat();
            _heartbeatCts = new CancellationTokenSource();
            RunHeartbeatLoop(_heartbeatCts.Token).Forget();
        }

        private void StopHeartbeat()
        {
            _waitingForPong = false;
            _heartbeatCts?.Cancel();
            _heartbeatCts?.Dispose();
            _heartbeatCts = null;
            _pongTimeoutCts?.Cancel();
            _pongTimeoutCts?.Dispose();
            _pongTimeoutCts = null;
        }

        private async UniTaskVoid RunHeartbeatLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_options.HeartbeatInterval),
                        cancellationToken: ct);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (ct.IsCancellationRequested)
                    return;

                try
                {
                    var pingJson = _serializer.Serialize(new { type = "ping" });
                    await _adapter.SendAsync(pingJson, ct);
                    // Only start pong timeout if not already waiting — avoid cancelling
                    // the previous timeout before it can fire
                    if (!_waitingForPong)
                    {
                        _waitingForPong = true;
                        StartPongTimeout(ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch
                {
                    // Send failed -- connection likely broken, HandleClose will deal with it
                    return;
                }
            }
        }

        private void StartPongTimeout(CancellationToken heartbeatCt)
        {
            _pongTimeoutCts?.Cancel();
            _pongTimeoutCts?.Dispose();
            _pongTimeoutCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(heartbeatCt, _pongTimeoutCts.Token);

            RunPongTimeout(linkedCts.Token, linkedCts).Forget();
        }

        private async UniTaskVoid RunPongTimeout(CancellationToken ct, CancellationTokenSource linkedCts)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_options.PongTimeout),
                    cancellationToken: ct);

                if (!ct.IsCancellationRequested && _waitingForPong)
                {
                    // Pong not received in time -- disconnect
                    _waitingForPong = false;
                    await _adapter.CloseAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Cancelled -- either pong was received or heartbeat stopped
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        private void OnPongReceived()
        {
            _waitingForPong = false;
            _pongTimeoutCts?.Cancel();
        }

        // === IDisposable ===

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _closedByUser = true;
            StopHeartbeat();

            _adapter.OnMessage -= HandleMessage;
            _adapter.OnClose -= HandleClose;
            _adapter.OnError -= HandleError;
            _adapter.OnOpen -= HandleOpen;
        }
    }
}
