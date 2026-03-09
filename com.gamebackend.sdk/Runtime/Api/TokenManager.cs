using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Interfaces;
using GameBackend.Core.Models;
using GameBackend.Transport.Interfaces;

namespace GameBackend.Api
{
    /// <summary>
    /// Manages JWT tokens: stores current session, persists to ITokenStorage,
    /// handles concurrent refresh via SemaphoreSlim.
    /// </summary>
    public class TokenManager
    {
        private readonly ISerializer _serializer;
        private readonly ITokenStorage _tokenStorage;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        public IGameSession CurrentSession { get; private set; }

        public TokenManager(ISerializer serializer, ITokenStorage tokenStorage)
        {
            _serializer = serializer;
            _tokenStorage = tokenStorage;
        }

        /// <summary>Load tokens from ITokenStorage and create GameSessionImpl.</summary>
        /// <returns>true if tokens were found and session was created</returns>
        public bool LoadFromStorage()
        {
            var (authToken, refreshToken) = _tokenStorage.Load();
            if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(refreshToken))
            {
                CurrentSession = null;
                return false;
            }

            CurrentSession = new GameSessionImpl(authToken, refreshToken);
            return true;
        }

        /// <summary>Save session tokens to ITokenStorage.</summary>
        public void SaveSession(IGameSession session)
        {
            _tokenStorage.Save(session.AuthToken, session.RefreshToken);
        }

        /// <summary>Set current session and persist to storage.</summary>
        public void SetSession(IGameSession session)
        {
            CurrentSession = session;
            SaveSession(session);
        }

        /// <summary>Clear current session and remove tokens from storage.</summary>
        public void Clear()
        {
            CurrentSession = null;
            _tokenStorage.Clear();
        }

        /// <summary>
        /// Refresh tokens via POST /api/auth/refresh.
        /// Uses SemaphoreSlim to ensure only one concurrent refresh.
        /// </summary>
        public async UniTask<IGameSession> RefreshAsync(IHttpAdapter httpAdapter, string baseUrl, CancellationToken ct = default)
        {
            await _refreshLock.WaitAsync(ct);
            try
            {
                var refreshToken = CurrentSession?.RefreshToken;
                if (string.IsNullOrEmpty(refreshToken))
                    throw new InvalidOperationException("No refresh token available");

                var body = _serializer.Serialize(new RefreshRequest { RefreshToken = refreshToken });
                var request = new HttpRequest
                {
                    Method = "POST",
                    Url = $"{baseUrl}/api/auth/refresh",
                    Body = body
                };
                request.Headers["Content-Type"] = "application/json";

                var response = await httpAdapter.SendAsync(request, ct);

                if (response.StatusCode >= 400)
                {
                    Clear();
                    return null;
                }

                var authResponse = _serializer.Deserialize<AuthResponse>(response.Body);
                var session = new GameSessionImpl(authResponse.AccessToken, authResponse.RefreshToken);
                SetSession(session);
                return session;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        [Newtonsoft.Json.JsonObject]
        internal class RefreshRequest
        {
            [Newtonsoft.Json.JsonProperty("refresh_token")]
            public string RefreshToken;
        }
    }
}
