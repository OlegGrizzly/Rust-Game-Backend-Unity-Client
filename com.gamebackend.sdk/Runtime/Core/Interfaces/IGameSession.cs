using System;

namespace GameBackend.Core.Interfaces
{
    public interface IGameSession
    {
        /// <summary>JWT access token</summary>
        string AuthToken { get; }

        /// <summary>JWT refresh token</summary>
        string RefreshToken { get; }

        /// <summary>User ID (from JWT claims sub)</summary>
        string UserId { get; }

        /// <summary>Username</summary>
        string Username { get; }

        /// <summary>Display name</summary>
        string DisplayName { get; }

        /// <summary>UNIX timestamp when access_token expires</summary>
        long ExpireTime { get; }

        /// <summary>UNIX timestamp when refresh_token expires</summary>
        long RefreshExpireTime { get; }

        /// <summary>Whether access_token is expired</summary>
        bool IsExpired { get; }

        /// <summary>Whether refresh_token is expired</summary>
        bool IsRefreshExpired { get; }

        /// <summary>Check expiration with offset (for pre-emptive refresh)</summary>
        bool HasExpired(DateTime offset);

        /// <summary>Check refresh token expiration with offset</summary>
        bool HasRefreshExpired(DateTime offset);
    }

    /// <summary>Static method for restoring session from saved tokens</summary>
    public static class GameSession
    {
        /// <summary>Restore session from previously saved tokens</summary>
        public static IGameSession Restore(string authToken, string refreshToken)
        {
            // Implementation will be provided by the Api layer
            throw new NotImplementedException("GameSession.Restore must be implemented by the Api layer.");
        }
    }
}
