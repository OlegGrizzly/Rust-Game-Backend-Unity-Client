using System;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class PresenceUpdate
    {
        [JsonProperty("user_id")]
        public string UserId;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("is_online")]
        public bool IsOnline;

        [JsonProperty("last_seen")]
        public DateTime? LastSeen;
    }

    public class BanInfo
    {
        [JsonProperty("reason")]
        public string Reason;

        [JsonProperty("banned_at")]
        public DateTime BannedAt;
    }

    public class WebSocketEnvelope
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("data")]
        public string Data;
    }
}
