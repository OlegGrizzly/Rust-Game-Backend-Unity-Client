using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class LeaderboardRecord
    {
        [JsonProperty("leaderboard_id")]
        public string LeaderboardId;

        [JsonProperty("rank")]
        public long Rank;

        [JsonProperty("user_id")]
        public string UserId;

        [JsonProperty("score")]
        public long Score;

        [JsonProperty("subscore")]
        public long Subscore;

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt;
    }

    public class LeaderboardRecordList
    {
        [JsonProperty("records")]
        public IReadOnlyList<LeaderboardRecord> Records;
    }
}
