using System;
using System.Linq;
using UnityEngine;
using GameBackend.Api;

namespace GameBackend.Samples
{
    /// <summary>
    /// Leaderboard Example -- manual testing of leaderboard operations.
    /// 1. First authenticate via AuthExample
    /// 2. Right-click -> Submit Score
    /// 3. Right-click -> Get Top Scores
    /// </summary>
    public class LeaderboardExample : MonoBehaviour
    {
        [SerializeField] private string scheme = "http";
        [SerializeField] private string host = "localhost";
        [SerializeField] private int port = 30080;
        [SerializeField] private string leaderboardId = "weekly_kills";
        [SerializeField] private long testScore = 1500;
        [TextArea(3, 10)]
        [SerializeField] private string lastResult = "";

        private GameClient _client;
        private GameClient Client => _client ?? (_client = new GameClient(scheme, host, port));

        [ContextMenu("Submit Score")]
        private async void SubmitScore()
        {
            try
            {
                await Client.WriteLeaderboardRecordAsync(leaderboardId, testScore);
                lastResult = $"Score {testScore} submitted to {leaderboardId}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Get Top Scores")]
        private async void GetTopScores()
        {
            try
            {
                var result = await Client.ListLeaderboardRecordsAsync(leaderboardId);
                var records = result.Records.ToList();
                lastResult = $"Top {records.Count} records:";
                foreach (var r in records) lastResult += $"\n  #{r.Rank} {r.UserId}: {r.Score}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Get Around Me")]
        private async void GetAroundMe()
        {
            try
            {
                var userId = Client.Session?.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    lastResult = "Not authenticated";
                    Debug.LogWarning(lastResult);
                    return;
                }
                var result = await Client.ListLeaderboardRecordsAroundUserAsync(leaderboardId, userId);
                var records = result.Records.ToList();
                lastResult = $"Around me ({records.Count} records):";
                foreach (var r in records) lastResult += $"\n  #{r.Rank} {r.UserId}: {r.Score}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }

        [ContextMenu("Delete My Record")]
        private async void DeleteMyRecord()
        {
            try
            {
                await Client.DeleteLeaderboardRecordAsync(leaderboardId);
                lastResult = $"Record deleted from {leaderboardId}";
                Debug.Log(lastResult);
            }
            catch (Exception e) { lastResult = $"Error: {e.Message}"; Debug.LogError(lastResult); }
        }
    }
}
