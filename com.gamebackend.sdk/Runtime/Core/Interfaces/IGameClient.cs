using GameBackend.Core;

namespace GameBackend.Core.Interfaces
{
    /// <summary>
    /// Facade combining all domain interfaces.
    /// Session is stored internally -- after auth all methods use it automatically.
    /// In gameplay code inject domain interfaces (ILeaderboardClient, etc), not IGameClient.
    /// </summary>
    public interface IGameClient :
        IAuthClient, IAccountClient, ILeaderboardClient,
        IChatClient, IStorageClient, IFriendsClient,
        IGroupsClient, INotificationClient, ITournamentClient
    {
        /// <summary>Current session (null if not authenticated)</summary>
        IGameSession Session { get; }

        /// <summary>Whether the client is authenticated</summary>
        bool IsAuthenticated { get; }

        /// <summary>Restore session from ITokenStorage (PlayerPrefs etc.)</summary>
        /// <returns>true if session was restored (tokens found)</returns>
        bool RestoreSession();

        /// <summary>Clear session and remove tokens from ITokenStorage</summary>
        void ClearSession();

        RetryConfiguration GlobalRetryConfiguration { get; set; }
        IGameSocket NewSocket();
    }
}
