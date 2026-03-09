using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Api.Models;
using GameBackend.Core.Interfaces;
using GameBackend.Core.Models;

namespace GameBackend.Api
{
    /// <summary>
    /// ILeaderboardClient implementation: submit/delete scores, top/around queries, batch lookup.
    /// </summary>
    public class LeaderboardService : ILeaderboardClient
    {
        private readonly HttpPipeline _pipeline;
        private readonly string _baseUrl;

        public LeaderboardService(HttpPipeline pipeline, string baseUrl)
        {
            _pipeline = pipeline;
            _baseUrl = baseUrl;
        }

        public async UniTask WriteLeaderboardRecordAsync(string leaderboardId, long score,
            long subscore = 0, Dictionary<string, object> metadata = null, CancellationToken ct = default)
        {
            var body = new WriteRecordRequest { Score = score, Subscore = subscore, Metadata = metadata };
            var request = new HttpRequest
            {
                Method = "POST",
                Url = $"{_baseUrl}/api/leaderboard/leaderboards/{leaderboardId}/record",
                Body = _pipeline.Serializer.Serialize(body)
            };
            request.Headers["Content-Type"] = "application/json";
            await _pipeline.SendAsync(request, ct);
        }

        public async UniTask<LeaderboardRecordList> ListLeaderboardRecordsAsync(string leaderboardId,
            int limit = 10, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Url = $"{_baseUrl}/api/leaderboard/leaderboards/{leaderboardId}?limit={limit}"
            };
            var records = await _pipeline.SendAsync<List<LeaderboardRecord>>(request, ct);
            return new LeaderboardRecordList { Records = records };
        }

        public async UniTask<LeaderboardRecordList> ListLeaderboardRecordsAroundUserAsync(string leaderboardId,
            string userId, int limit = 10, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Url = $"{_baseUrl}/api/leaderboard/leaderboards/{leaderboardId}/around/{userId}?limit={limit}"
            };
            var records = await _pipeline.SendAsync<List<LeaderboardRecord>>(request, ct);
            return new LeaderboardRecordList { Records = records };
        }

        public async UniTask DeleteLeaderboardRecordAsync(string leaderboardId, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "DELETE",
                Url = $"{_baseUrl}/api/leaderboard/leaderboards/{leaderboardId}/record"
            };
            await _pipeline.SendAsync(request, ct);
        }

        public async UniTask<IEnumerable<LeaderboardRecord>> GetLeaderboardRecordsByIdsAsync(string leaderboardId,
            IEnumerable<string> userIds, CancellationToken ct = default)
        {
            var body = new BatchRecordIdsRequest { UserIds = userIds.ToArray() };
            var request = new HttpRequest
            {
                Method = "POST",
                Url = $"{_baseUrl}/api/leaderboard/leaderboards/{leaderboardId}/records/batch",
                Body = _pipeline.Serializer.Serialize(body)
            };
            request.Headers["Content-Type"] = "application/json";
            return await _pipeline.SendAsync<List<LeaderboardRecord>>(request, ct);
        }
    }
}
