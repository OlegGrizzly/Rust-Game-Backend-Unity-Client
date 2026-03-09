using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Core.Interfaces
{
    public interface IFriendsClient
    {
        UniTask AddFriendAsync(string userId, CancellationToken ct = default);
        UniTask AcceptFriendAsync(string userId, CancellationToken ct = default);
        UniTask RejectFriendAsync(string userId, CancellationToken ct = default);
        UniTask RemoveFriendAsync(string userId, CancellationToken ct = default);
        UniTask BlockFriendAsync(string userId, CancellationToken ct = default);
        UniTask UnblockFriendAsync(string userId, CancellationToken ct = default);
        UniTask<IEnumerable<Friend>> ListFriendsAsync(CancellationToken ct = default);
        UniTask<IEnumerable<FriendRequest>> ListIncomingFriendsAsync(CancellationToken ct = default);
        UniTask<IEnumerable<FriendRequest>> ListOutgoingFriendsAsync(CancellationToken ct = default);
        UniTask<IEnumerable<Friend>> ListBlockedAsync(CancellationToken ct = default);
    }
}
