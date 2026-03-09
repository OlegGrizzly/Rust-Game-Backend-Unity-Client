using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class LeaderboardRecord
    {
        [JsonProperty("rank")]
        public long Rank;

        [JsonProperty("user_id")]
        public string UserId;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("score")]
        public long Score;

        [JsonProperty("subscore")]
        public long Subscore;

        [JsonProperty("num_score")]
        public int NumScore;

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt;
    }

    public class LeaderboardRecordList
    {
        [JsonProperty("records")]
        public IReadOnlyList<LeaderboardRecord> Records;
    }
}
