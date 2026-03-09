using System;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class Friend
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("display_name")]
        public string DisplayName;

        [JsonProperty("avatar_url")]
        public string AvatarUrl;

        [JsonProperty("since")]
        public DateTime Since;
    }

    public class FriendRequest
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("display_name")]
        public string DisplayName;

        [JsonProperty("avatar_url")]
        public string AvatarUrl;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;
    }

    public class FriendEvent
    {
        [JsonProperty("user_id")]
        public string UserId;

        [JsonProperty("username")]
        public string Username;
    }
}
