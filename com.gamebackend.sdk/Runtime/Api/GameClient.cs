using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core;
using GameBackend.Core.Interfaces;
using GameBackend.Core.Models;
using GameBackend.Transport;
using GameBackend.Transport.Http;
using GameBackend.Transport.Storage;
using GameBackend.Transport.Interfaces;

namespace GameBackend.Api
{
    /// <summary>
    /// Facade implementing IGameClient. Combines all domain interfaces.
    /// Auth methods delegate to AuthService; other domains throw NotImplementedException (Phase 3+).
    /// </summary>
    public class GameClient : IGameClient
    {
        private readonly TokenManager _tokenManager;
        private readonly HttpPipeline _pipeline;
        private readonly AuthService _authService;
        private readonly AccountService _accountService;
        private readonly LeaderboardService _leaderboardService;

        public IGameSession Session => _tokenManager.CurrentSession;
        public bool IsAuthenticated => _tokenManager.CurrentSession != null && !_tokenManager.CurrentSession.IsExpired;
        public RetryConfiguration GlobalRetryConfiguration { get; set; }

        public GameClient(string scheme, string host, int port,
            IHttpAdapter httpAdapter = null,
            ISerializer serializer = null,
            ITokenStorage tokenStorage = null)
        {
            var baseUrl = $"{scheme}://{host}:{port}";

            httpAdapter = httpAdapter ?? new UnityWebRequestAdapter();
            serializer = serializer ?? new NewtonsoftSerializer();
            tokenStorage = tokenStorage ?? new GameBackend.Transport.Storage.PlayerPrefsTokenStorage();
            _tokenManager = new TokenManager(serializer, tokenStorage);
            _pipeline = new HttpPipeline(httpAdapter, serializer, _tokenManager);
            _authService = new AuthService(_pipeline, _tokenManager, baseUrl);
            _accountService = new AccountService(_pipeline, baseUrl);
            _leaderboardService = new LeaderboardService(_pipeline, baseUrl);
        }

        // =====================================================================
        // Session management
        // =====================================================================

        public bool RestoreSession()
        {
            return _tokenManager.LoadFromStorage();
        }

        public void ClearSession()
        {
            _tokenManager.Clear();
        }

        // =====================================================================
        // IAuthClient (delegated to AuthService)
        // =====================================================================

        public UniTask<IGameSession> AuthenticateUsernameAsync(string username, string password, CancellationToken ct = default)
            => _authService.AuthenticateUsernameAsync(username, password, ct);

        public UniTask<IGameSession> AuthenticateEmailAsync(string email, string password, string username, CancellationToken ct = default)
            => _authService.AuthenticateEmailAsync(email, password, username, ct);

        public UniTask<IGameSession> AuthenticateDeviceAsync(string deviceId, CancellationToken ct = default)
            => _authService.AuthenticateDeviceAsync(deviceId, ct);

        public UniTask<IGameSession> LoginUsernameAsync(string username, string password, CancellationToken ct = default)
            => _authService.LoginUsernameAsync(username, password, ct);

        public UniTask<IGameSession> LoginEmailAsync(string email, string password, CancellationToken ct = default)
            => _authService.LoginEmailAsync(email, password, ct);

        public UniTask<IGameSession> LoginDeviceAsync(string deviceId, CancellationToken ct = default)
            => _authService.LoginDeviceAsync(deviceId, ct);

        public UniTask<IGameSession> RefreshSessionAsync(CancellationToken ct = default)
            => _authService.RefreshSessionAsync(ct);

        public UniTask LogoutAsync(CancellationToken ct = default)
            => _authService.LogoutAsync(ct);

        public UniTask<IReadOnlyList<SessionInfo>> ListSessionsAsync(CancellationToken ct = default)
            => _authService.ListSessionsAsync(ct);

        public UniTask RevokeSessionAsync(string sessionId, CancellationToken ct = default)
            => _authService.RevokeSessionAsync(sessionId, ct);

        public UniTask RevokeAllSessionsAsync(CancellationToken ct = default)
            => _authService.RevokeAllSessionsAsync(ct);

        public UniTask LinkProviderAsync(string provider, string providerId, string secret = null, CancellationToken ct = default)
            => _authService.LinkProviderAsync(provider, providerId, secret, ct);

        public UniTask UnlinkProviderAsync(string provider, CancellationToken ct = default)
            => _authService.UnlinkProviderAsync(provider, ct);

        // =====================================================================
        // IGameSocket
        // =====================================================================

        public IGameSocket NewSocket() => throw new NotImplementedException("WebSocket not implemented yet (Phase 4)");

        // =====================================================================
        // IAccountClient (Phase 3)
        // =====================================================================

        public UniTask<Account> GetAccountAsync(CancellationToken ct = default)
            => _accountService.GetAccountAsync(ct);

        public UniTask UpdateAccountAsync(string displayName = null, string avatarUrl = null,
            string lang = null, string location = null, string timezone = null, CancellationToken ct = default)
            => _accountService.UpdateAccountAsync(displayName, avatarUrl, lang, location, timezone, ct);

        public UniTask DeleteAccountAsync(CancellationToken ct = default)
            => _accountService.DeleteAccountAsync(ct);

        public UniTask<User> GetUserAsync(string userId, CancellationToken ct = default)
            => _accountService.GetUserAsync(userId, ct);

        public UniTask<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userIds, CancellationToken ct = default)
            => _accountService.GetUsersAsync(userIds, ct);

        // =====================================================================
        // ILeaderboardClient (Phase 3)
        // =====================================================================

        public UniTask WriteLeaderboardRecordAsync(string leaderboardId, long score,
            long subscore = 0, Dictionary<string, object> metadata = null,
            CancellationToken ct = default)
            => _leaderboardService.WriteLeaderboardRecordAsync(leaderboardId, score, subscore, metadata, ct);

        public UniTask<LeaderboardRecordList> ListLeaderboardRecordsAsync(string leaderboardId, int limit = 10, CancellationToken ct = default)
            => _leaderboardService.ListLeaderboardRecordsAsync(leaderboardId, limit, ct);

        public UniTask<LeaderboardRecordList> ListLeaderboardRecordsAroundUserAsync(string leaderboardId, string userId, int limit = 10, CancellationToken ct = default)
            => _leaderboardService.ListLeaderboardRecordsAroundUserAsync(leaderboardId, userId, limit, ct);

        public UniTask DeleteLeaderboardRecordAsync(string leaderboardId, CancellationToken ct = default)
            => _leaderboardService.DeleteLeaderboardRecordAsync(leaderboardId, ct);

        public UniTask<IEnumerable<LeaderboardRecord>> GetLeaderboardRecordsByIdsAsync(string leaderboardId, IEnumerable<string> userIds, CancellationToken ct = default)
            => _leaderboardService.GetLeaderboardRecordsByIdsAsync(leaderboardId, userIds, ct);

        // =====================================================================
        // IChatClient (Phase 4)
        // =====================================================================

        public UniTask<IEnumerable<Core.Models.Channel>> ListChannelsAsync(CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask<Core.Models.Channel> CreateChannelAsync(CreateChannelRequest request, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask<MessageList> ListMessagesAsync(string channelId, int limit = 50, string cursor = null, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask<UnreadInfo> GetUnreadAsync(string channelId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask MarkChannelReadAsync(string channelId, CancellationToken ct = default)
            => throw new NotImplementedException();

        // =====================================================================
        // IStorageClient (Phase 5)
        // =====================================================================

        public UniTask WriteStorageObjectsAsync(IEnumerable<StorageObjectWrite> objects, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask<IEnumerable<StorageObject>> ReadStorageObjectsAsync(IEnumerable<StorageObjectId> keys, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask DeleteStorageObjectAsync(string collection, string key, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask<IEnumerable<StorageObject>> SearchStorageObjectsAsync(string collection, string query, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask<int> CountStorageObjectsAsync(string collection, string key = null, CancellationToken ct = default)
            => throw new NotImplementedException();

        // =====================================================================
        // IFriendsClient (Phase 5)
        // =====================================================================

        public UniTask AddFriendAsync(string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask AcceptFriendAsync(string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask RejectFriendAsync(string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask RemoveFriendAsync(string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask BlockFriendAsync(string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask UnblockFriendAsync(string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<IEnumerable<Friend>> ListFriendsAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<IEnumerable<FriendRequest>> ListIncomingFriendsAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<IEnumerable<FriendRequest>> ListOutgoingFriendsAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<IEnumerable<Friend>> ListBlockedAsync(CancellationToken ct = default) => throw new NotImplementedException();

        // =====================================================================
        // IGroupsClient (Phase 5)
        // =====================================================================

        public UniTask<Group> CreateGroupAsync(string name, string description = "",
            string avatarUrl = null, bool open = false, int maxCount = 100,
            Dictionary<string, object> metadata = null, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask<Group> UpdateGroupAsync(string groupId, string name = null,
            string description = null, string avatarUrl = null, bool? open = null,
            int? maxCount = null, Dictionary<string, object> metadata = null, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask DeleteGroupAsync(string groupId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask JoinGroupAsync(string groupId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask RequestJoinGroupAsync(string groupId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask AcceptJoinRequestAsync(string groupId, string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask RejectJoinRequestAsync(string groupId, string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<IEnumerable<GroupJoinRequest>> ListJoinRequestsAsync(string groupId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask KickMemberAsync(string groupId, string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask PromoteMemberAsync(string groupId, string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask DemoteMemberAsync(string groupId, string userId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask LeaveGroupAsync(string groupId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<IEnumerable<GroupMember>> ListMembersAsync(string groupId, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<IEnumerable<Group>> SearchGroupsAsync(string name, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<IEnumerable<MyGroup>> ListMyGroupsAsync(CancellationToken ct = default) => throw new NotImplementedException();

        // =====================================================================
        // INotificationClient (Phase 5)
        // =====================================================================

        public UniTask<NotificationList> ListNotificationsAsync(int start = 0, int end = 25, bool unreadOnly = false, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask<int> GetUnreadNotificationCountAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<int> MarkNotificationsReadAsync(IEnumerable<string> ids, CancellationToken ct = default) => throw new NotImplementedException();
        public UniTask<int> DeleteNotificationsAsync(IEnumerable<string> ids, CancellationToken ct = default) => throw new NotImplementedException();

        // =====================================================================
        // ITournamentClient (Phase 5)
        // =====================================================================

        public UniTask<IEnumerable<Tournament>> ListTournamentsAsync(int limit = 50, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask<TournamentDetails> GetTournamentAsync(string tournamentId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public UniTask JoinTournamentAsync(string tournamentId, CancellationToken ct = default) => throw new NotImplementedException();

        public UniTask<TournamentRecord> WriteTournamentRecordAsync(string tournamentId,
            long score, long subscore = 0, Dictionary<string, object> metadata = null, CancellationToken ct = default)
            => throw new NotImplementedException();
    }

}
