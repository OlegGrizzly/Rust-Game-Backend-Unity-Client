using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameBackend.Core.Models
{
    public class PresenceUpdate
    {
        [JsonProperty("user_id")]
        public string UserId;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("online")]
        public bool Online;

        [JsonProperty("status")]
        public string Status;
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
        public JToken Data;
    }

    public class ChannelEvent
    {
        [JsonProperty("channel_id")]
        public string ChannelId;

        [JsonProperty("user_id")]
        public string UserId;
    }

    public class ChannelMembersEvent
    {
        [JsonProperty("channel_id")]
        public string ChannelId;

        [JsonProperty("members")]
        public string[] Members;
    }

    public class PresenceStateEvent
    {
        [JsonProperty("users")]
        public IReadOnlyList<PresenceUpdate> Users;
    }

    public class SessionRevokedEvent
    {
        [JsonProperty("session_id")]
        public string SessionId;

        [JsonProperty("reason")]
        public string Reason;
    }

    public class UserBannedEvent
    {
        [JsonProperty("reason")]
        public string Reason;

        [JsonProperty("expires_at")]
        public string ExpiresAt;
    }

    public class NotificationEvent
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("content")]
        public string Content;

        [JsonProperty("created_at")]
        public string CreatedAt;
    }
}
