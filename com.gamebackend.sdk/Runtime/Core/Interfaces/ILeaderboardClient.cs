using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Core.Interfaces
{
    public interface ILeaderboardClient
    {
        UniTask WriteLeaderboardRecordAsync(string leaderboardId, long score, CancellationToken ct = default);
        UniTask<LeaderboardRecordList> ListLeaderboardRecordsAsync(string leaderboardId, int limit = 10, CancellationToken ct = default);
        UniTask<LeaderboardRecordList> ListLeaderboardRecordsAroundUserAsync(string leaderboardId, string userId, CancellationToken ct = default);
        UniTask DeleteLeaderboardRecordAsync(string leaderboardId, CancellationToken ct = default);
        UniTask<IEnumerable<LeaderboardRecord>> GetLeaderboardRecordsByIdsAsync(string leaderboardId, IEnumerable<string> userIds, CancellationToken ct = default);
    }
}
