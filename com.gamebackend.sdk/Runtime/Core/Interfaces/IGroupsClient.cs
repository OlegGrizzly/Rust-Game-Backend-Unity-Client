using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Core.Interfaces
{
    public interface IGroupsClient
    {
        UniTask<Group> CreateGroupAsync(string name, string description = "",
            string avatarUrl = null, bool open = false, int maxCount = 100,
            Dictionary<string, object> metadata = null, CancellationToken ct = default);
        UniTask<Group> UpdateGroupAsync(string groupId, string name = null,
            string description = null, string avatarUrl = null, bool? open = null,
            int? maxCount = null, Dictionary<string, object> metadata = null, CancellationToken ct = default);
        UniTask DeleteGroupAsync(string groupId, CancellationToken ct = default);
        UniTask JoinGroupAsync(string groupId, CancellationToken ct = default);
        UniTask RequestJoinGroupAsync(string groupId, CancellationToken ct = default);
        UniTask AcceptJoinRequestAsync(string groupId, string userId, CancellationToken ct = default);
        UniTask RejectJoinRequestAsync(string groupId, string userId, CancellationToken ct = default);
        UniTask<IEnumerable<GroupJoinRequest>> ListJoinRequestsAsync(string groupId, CancellationToken ct = default);
        UniTask KickMemberAsync(string groupId, string userId, CancellationToken ct = default);
        UniTask PromoteMemberAsync(string groupId, string userId, CancellationToken ct = default);
        UniTask DemoteMemberAsync(string groupId, string userId, CancellationToken ct = default);
        UniTask LeaveGroupAsync(string groupId, CancellationToken ct = default);
        UniTask<IEnumerable<GroupMember>> ListMembersAsync(string groupId, CancellationToken ct = default);
        UniTask<IEnumerable<Group>> SearchGroupsAsync(string name, CancellationToken ct = default);
        UniTask<IEnumerable<MyGroup>> ListMyGroupsAsync(CancellationToken ct = default);
    }
}
