using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Core.Interfaces
{
    public interface IAccountClient
    {
        UniTask<Account> GetAccountAsync(CancellationToken ct = default);
        UniTask UpdateAccountAsync(string displayName = null, string avatarUrl = null,
            string lang = null, string location = null, string timezone = null, CancellationToken ct = default);
        UniTask DeleteAccountAsync(CancellationToken ct = default);
        UniTask<User> GetUserAsync(string userId, CancellationToken ct = default);
        UniTask<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userIds, CancellationToken ct = default);
    }
}
