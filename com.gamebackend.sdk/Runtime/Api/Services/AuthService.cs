using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Api.Models;
using GameBackend.Core.Interfaces;
using GameBackend.Core.Models;

namespace GameBackend.Api
{
    /// <summary>
    /// IAuthClient implementation: register, login, refresh, logout, session management.
    /// All auth/login methods deserialize AuthResponse, create GameSessionImpl, save via TokenManager.
    /// </summary>
    public class AuthService : IAuthClient
    {
        private readonly HttpPipeline _pipeline;
        private readonly TokenManager _tokenManager;
        private readonly string _baseUrl;

        public AuthService(HttpPipeline pipeline, TokenManager tokenManager, string baseUrl)
        {
            _pipeline = pipeline;
            _tokenManager = tokenManager;
            _baseUrl = baseUrl;
            _pipeline.SetBaseUrl(baseUrl);
        }

        // =====================================================================
        // Register (Authenticate)
        // =====================================================================

        public async UniTask<IGameSession> AuthenticateUsernameAsync(string username, string password, CancellationToken ct = default)
        {
            var request = CreatePostRequest($"{_baseUrl}/api/auth/register",
                new RegisterUsernameRequest { Username = username, Password = password });
            return await SendAuthRequest(request, ct);
        }

        public async UniTask<IGameSession> AuthenticateEmailAsync(string email, string password, string username, CancellationToken ct = default)
        {
            var request = CreatePostRequest($"{_baseUrl}/api/auth/register",
                new RegisterEmailRequest { Email = email, Password = password, Username = username });
            return await SendAuthRequest(request, ct);
        }

        public async UniTask<IGameSession> AuthenticateDeviceAsync(string deviceId, CancellationToken ct = default)
        {
            var request = CreatePostRequest($"{_baseUrl}/api/auth/register",
                new RegisterDeviceRequest { DeviceId = deviceId });
            return await SendAuthRequest(request, ct);
        }

        // =====================================================================
        // Login
        // =====================================================================

        public async UniTask<IGameSession> LoginUsernameAsync(string username, string password, CancellationToken ct = default)
        {
            var request = CreatePostRequest($"{_baseUrl}/api/auth/login",
                new LoginUsernameRequest { Username = username, Password = password });
            return await SendAuthRequest(request, ct);
        }

        public async UniTask<IGameSession> LoginEmailAsync(string email, string password, CancellationToken ct = default)
        {
            var request = CreatePostRequest($"{_baseUrl}/api/auth/login",
                new LoginEmailRequest { Email = email, Password = password });
            return await SendAuthRequest(request, ct);
        }

        public async UniTask<IGameSession> LoginDeviceAsync(string deviceId, CancellationToken ct = default)
        {
            var request = CreatePostRequest($"{_baseUrl}/api/auth/login",
                new LoginDeviceRequest { DeviceId = deviceId });
            return await SendAuthRequest(request, ct);
        }

        // =====================================================================
        // Refresh
        // =====================================================================

        public async UniTask<IGameSession> RefreshSessionAsync(CancellationToken ct = default)
        {
            var refreshToken = _tokenManager.CurrentSession?.RefreshToken;
            var request = CreatePostRequest($"{_baseUrl}/api/auth/refresh",
                new TokenManager.RefreshRequest { RefreshToken = refreshToken });
            return await SendAuthRequest(request, ct);
        }

        // =====================================================================
        // Logout / Session management
        // =====================================================================

        public async UniTask LogoutAsync(CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "DELETE",
                Url = $"{_baseUrl}/api/auth/sessions"
            };
            await _pipeline.SendAsync(request, ct);
            _tokenManager.Clear();
        }

        public async UniTask RevokeAllSessionsAsync(CancellationToken ct = default)
        {
            await LogoutAsync(ct);
        }

        public async UniTask<IReadOnlyList<SessionInfo>> ListSessionsAsync(CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Url = $"{_baseUrl}/api/auth/sessions"
            };
            var list = await _pipeline.SendAsync<List<SessionInfo>>(request, ct);
            return list;
        }

        public async UniTask RevokeSessionAsync(string sessionId, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "DELETE",
                Url = $"{_baseUrl}/api/auth/sessions/{sessionId}"
            };
            await _pipeline.SendAsync(request, ct);
        }

        public async UniTask LinkProviderAsync(string provider, string providerId, string secret = null, CancellationToken ct = default)
        {
            var request = CreatePostRequest($"{_baseUrl}/api/auth/providers/link",
                new LinkProviderRequest { Provider = provider, ProviderId = providerId, Secret = secret });
            await _pipeline.SendAsync(request, ct);
        }

        public async UniTask UnlinkProviderAsync(string provider, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "DELETE",
                Url = $"{_baseUrl}/api/auth/providers/{provider}"
            };
            await _pipeline.SendAsync(request, ct);
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private HttpRequest CreatePostRequest<T>(string url, T body)
        {
            var request = new HttpRequest
            {
                Method = "POST",
                Url = url,
                Body = _pipeline.Serializer.Serialize(body)
            };
            request.Headers["Content-Type"] = "application/json";
            return request;
        }

        private async UniTask<IGameSession> SendAuthRequest(HttpRequest request, CancellationToken ct)
        {
            var authResponse = await _pipeline.SendAsync<AuthResponse>(request, ct);
            var session = new GameSessionImpl(authResponse.AccessToken, authResponse.RefreshToken);
            _tokenManager.SetSession(session);
            return session;
        }
    }
}
