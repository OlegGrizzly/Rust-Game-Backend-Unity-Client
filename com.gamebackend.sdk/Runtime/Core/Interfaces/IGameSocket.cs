using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Core.Interfaces
{
    public interface IGameSocket : IDisposable
    {
        // === State ===
        bool IsConnected { get; }

        // === Events (C# events -- SDK core) ===
        event Action Connected;
        event Action Closed;
        event Action<Exception> ReceivedError;
        event Action<ChatMessage> ReceivedChatMessage;
        event Action<PresenceUpdate> ReceivedPresenceUpdate;
        event Action<Notification> ReceivedNotification;
        event Action ReceivedSessionRevoked;
        event Action<BanInfo> ReceivedUserBanned;
        event Action<FriendEvent> ReceivedFriendRequest;
        event Action<FriendEvent> ReceivedFriendAccepted;
        event Action<GroupEvent> ReceivedGroupJoined;
        event Action<GroupEvent> ReceivedGroupKicked;
        event Action<TournamentEvent> ReceivedTournamentStarted;
        event Action<TournamentEvent> ReceivedTournamentEnded;

        // === Methods ===

        /// <summary>Connect to WebSocket (takes access_token from GameClient.Session)</summary>
        UniTask ConnectAsync(CancellationToken ct = default);

        /// <summary>Disconnect</summary>
        UniTask CloseAsync();

        /// <summary>Send chat message via WebSocket</summary>
        UniTask SendChatMessageAsync(
            string channelId,
            string content,
            CancellationToken ct = default);
    }
}
