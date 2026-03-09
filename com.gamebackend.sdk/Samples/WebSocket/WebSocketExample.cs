using System;
using UnityEngine;
using GameBackend.Api;
using GameBackend.Core.Models;
using GameBackend.Transport;
using GameBackend.Transport.WebSocket;
using GameBackend.WebSocket;

namespace GameBackend.Samples
{
    /// <summary>
    /// WebSocket Example -- manual testing of real-time WebSocket connection.
    /// 1. First authenticate via AuthExample (tokens saved to PlayerPrefs)
    /// 2. Right-click -> Restore Session
    /// 3. Right-click -> Connect WebSocket
    /// 4. Right-click -> Send Chat Message
    /// 5. Right-click -> Disconnect
    /// </summary>
    public class WebSocketExample : MonoBehaviour
    {
        [SerializeField] private string scheme = "http";
        [SerializeField] private string host = "localhost";
        [SerializeField] private int port = 30080;
        [SerializeField] private string channelId = "";
        [SerializeField] private string messageText = "Hello from Unity!";
        [TextArea(3, 10)]
        [SerializeField] private string lastResult = "";

        private GameClient _client;
        private GameClient Client => _client ?? (_client = new GameClient(scheme, host, port));

        private GameSocket _socket;
        private NativeWebSocketAdapter _adapter;

        [ContextMenu("Restore Session")]
        private void RestoreSession()
        {
            var restored = Client.RestoreSession();
            lastResult = restored
                ? $"Session restored: {Client.Session.UserId}"
                : "No saved session found";
            Debug.Log(lastResult);
        }

        [ContextMenu("Connect WebSocket")]
        private async void ConnectWebSocket()
        {
            try
            {
                if (Client.Session == null || string.IsNullOrEmpty(Client.Session.AuthToken))
                {
                    lastResult = "Not authenticated. Restore session first.";
                    Debug.LogWarning(lastResult);
                    return;
                }

                // Clean up previous connection if any
                CleanupSocket();

                _adapter = new NativeWebSocketAdapter();
                var serializer = new NewtonsoftSerializer();
                _socket = new GameSocket(_adapter, serializer, new GameSocketOptions
                {
                    Host = $"{host}:{port}",
                    AccessToken = Client.Session.AuthToken
                });

                // Subscribe to events
                _socket.Connected += OnConnected;
                _socket.Closed += OnClosed;
                _socket.ReceivedError += OnError;
                _socket.ReceivedChatMessage += OnChatMessage;
                _socket.ReceivedPresenceUpdate += OnPresenceUpdate;
                _socket.ReceivedNotification += OnNotification;
                _socket.ReceivedFriendRequest += OnFriendRequest;
                _socket.ReceivedFriendAccepted += OnFriendAccepted;
                _socket.ReceivedChannelJoined += OnChannelJoined;
                _socket.ReceivedChannelLeft += OnChannelLeft;
                _socket.ReceivedSessionRevoked += OnSessionRevoked;
                _socket.ReceivedUserBanned += OnUserBanned;

                await _socket.ConnectAsync();
                lastResult = "Connecting...";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Send Chat Message")]
        private async void SendChatMessage()
        {
            try
            {
                if (_socket == null || !_socket.IsConnected)
                {
                    lastResult = "WebSocket not connected. Connect first.";
                    Debug.LogWarning(lastResult);
                    return;
                }

                if (string.IsNullOrEmpty(channelId))
                {
                    lastResult = "channelId is empty. Set it in Inspector.";
                    Debug.LogWarning(lastResult);
                    return;
                }

                await _socket.SendChatMessageAsync(channelId, messageText);
                lastResult = $"Sent to {channelId}: {messageText}";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Disconnect")]
        private async void Disconnect()
        {
            try
            {
                if (_socket == null)
                {
                    lastResult = "No active connection";
                    Debug.LogWarning(lastResult);
                    return;
                }

                await _socket.CloseAsync();
                lastResult = "Disconnected";
                Debug.Log(lastResult);
            }
            catch (Exception e)
            {
                lastResult = $"Error: {e.Message}";
                Debug.LogError(lastResult);
            }
        }

        [ContextMenu("Check Connection Status")]
        private void CheckConnectionStatus()
        {
            var connected = _socket != null && _socket.IsConnected;
            lastResult = $"IsConnected: {connected}";
            Debug.Log(lastResult);
        }

        // NativeWebSocket requires dispatching messages every frame
        private void Update()
        {
            _adapter?.DispatchMessageQueue();
        }

        private void OnDestroy()
        {
            CleanupSocket();
        }

        private void CleanupSocket()
        {
            if (_socket != null)
            {
                _socket.Connected -= OnConnected;
                _socket.Closed -= OnClosed;
                _socket.ReceivedError -= OnError;
                _socket.ReceivedChatMessage -= OnChatMessage;
                _socket.ReceivedPresenceUpdate -= OnPresenceUpdate;
                _socket.ReceivedNotification -= OnNotification;
                _socket.ReceivedFriendRequest -= OnFriendRequest;
                _socket.ReceivedFriendAccepted -= OnFriendAccepted;
                _socket.ReceivedChannelJoined -= OnChannelJoined;
                _socket.ReceivedChannelLeft -= OnChannelLeft;
                _socket.ReceivedSessionRevoked -= OnSessionRevoked;
                _socket.ReceivedUserBanned -= OnUserBanned;
                _socket.Dispose();
                _socket = null;
            }

            _adapter = null;
        }

        // === Event Handlers ===

        private void OnConnected()
        {
            lastResult = "WebSocket connected";
            Debug.Log($"[WS] {lastResult}");
        }

        private void OnClosed()
        {
            lastResult = "WebSocket closed";
            Debug.Log($"[WS] {lastResult}");
        }

        private void OnError(Exception error)
        {
            lastResult = $"WS Error: {error.Message}";
            Debug.LogError($"[WS] {lastResult}");
        }

        private void OnChatMessage(ChatMessage msg)
        {
            lastResult = $"Chat [{msg.ChannelId}] {msg.SenderName}: {msg.Content}";
            Debug.Log($"[WS] {lastResult}");
        }

        private void OnPresenceUpdate(PresenceUpdate update)
        {
            lastResult = $"Presence: {update.UserId} -> {update.Status}";
            Debug.Log($"[WS] {lastResult}");
        }

        private void OnNotification(NotificationEvent notification)
        {
            lastResult = $"Notification [{notification.Type}]: {notification.Content}";
            Debug.Log($"[WS] {lastResult}");
        }

        private void OnFriendRequest(FriendEvent evt)
        {
            lastResult = $"Friend request from: {evt.UserId}";
            Debug.Log($"[WS] {lastResult}");
        }

        private void OnFriendAccepted(FriendEvent evt)
        {
            lastResult = $"Friend accepted: {evt.UserId}";
            Debug.Log($"[WS] {lastResult}");
        }

        private void OnChannelJoined(ChannelEvent evt)
        {
            lastResult = $"Channel joined: {evt.ChannelId}";
            Debug.Log($"[WS] {lastResult}");
        }

        private void OnChannelLeft(ChannelEvent evt)
        {
            lastResult = $"Channel left: {evt.ChannelId}";
            Debug.Log($"[WS] {lastResult}");
        }

        private void OnSessionRevoked(SessionRevokedEvent evt)
        {
            lastResult = $"Session revoked: {evt.Reason}";
            Debug.LogWarning($"[WS] {lastResult}");
        }

        private void OnUserBanned(UserBannedEvent evt)
        {
            lastResult = $"User banned: {evt.Reason}";
            Debug.LogWarning($"[WS] {lastResult}");
        }
    }
}
