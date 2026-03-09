using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Core.Interfaces
{
    public interface IAuthClient
    {
        UniTask<IGameSession> AuthenticateUsernameAsync(string username, string password, CancellationToken ct = default);
        UniTask<IGameSession> AuthenticateEmailAsync(string email, string password, string username, CancellationToken ct = default);
        UniTask<IGameSession> AuthenticateDeviceAsync(string deviceId, CancellationToken ct = default);
        UniTask<IGameSession> LoginUsernameAsync(string username, string password, CancellationToken ct = default);
        UniTask<IGameSession> LoginEmailAsync(string email, string password, CancellationToken ct = default);
        UniTask<IGameSession> LoginDeviceAsync(string deviceId, CancellationToken ct = default);
        UniTask<IGameSession> RefreshSessionAsync(CancellationToken ct = default);
        UniTask LogoutAsync(CancellationToken ct = default);

        // Additional methods from REST API
        UniTask<IReadOnlyList<SessionInfo>> ListSessionsAsync(CancellationToken ct = default);
        UniTask RevokeSessionAsync(string sessionId, CancellationToken ct = default);
        UniTask RevokeAllSessionsAsync(CancellationToken ct = default);
        UniTask LinkProviderAsync(string provider, string providerId, string secret = null, CancellationToken ct = default);
        UniTask UnlinkProviderAsync(string provider, CancellationToken ct = default);
    }
}
