using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class Tournament
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("start_time")]
        public DateTime StartTime;

        [JsonProperty("end_time")]
        public DateTime EndTime;

        [JsonProperty("max_size")]
        public int MaxSize;

        [JsonProperty("max_num_score")]
        public int MaxNumScore;

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("sort_order")]
        public string SortOrder;

        [JsonProperty("operator")]
        public string Operator;

        [JsonProperty("leaderboard_id")]
        public string LeaderboardId;

        [JsonProperty("rewards")]
        public IReadOnlyList<TournamentReward> Rewards;

        [JsonProperty("participant_count")]
        public long ParticipantCount;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;
    }

    public class TournamentReward
    {
        [JsonProperty("label")]
        public string Label;

        [JsonProperty("code")]
        public string Code;
    }

    public class TournamentRecord
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

    public class TournamentDetails
    {
        [JsonProperty("tournament")]
        public Tournament Tournament;

        [JsonProperty("records")]
        public IReadOnlyList<TournamentRecord> Records;

        [JsonProperty("user_record")]
        public TournamentRecord UserRecord;
    }

    public class TournamentEvent
    {
        [JsonProperty("tournament_id")]
        public string TournamentId;

        [JsonProperty("title")]
        public string Title;
    }
}
