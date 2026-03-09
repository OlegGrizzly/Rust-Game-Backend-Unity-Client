using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class Notification
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("user_id")]
        public string UserId;

        [JsonProperty("subject")]
        public string Subject;

        [JsonProperty("content")]
        public string Content;

        [JsonProperty("code")]
        public string Code;

        [JsonProperty("sender_id")]
        public string SenderId;

        [JsonProperty("read")]
        public bool Read;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;
    }

    public class NotificationList
    {
        [JsonProperty("notifications")]
        public IReadOnlyList<Notification> Notifications;

        [JsonProperty("total_count")]
        public int TotalCount;
    }
}
