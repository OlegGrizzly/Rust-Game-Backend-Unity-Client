using System;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class Account
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("display_name")]
        public string DisplayName;

        [JsonProperty("avatar_url")]
        public string AvatarUrl;

        [JsonProperty("lang")]
        public string Lang;

        [JsonProperty("location")]
        public string Location;

        [JsonProperty("timezone")]
        public string Timezone;

        [JsonProperty("is_banned")]
        public bool IsBanned;

        [JsonProperty("ban_reason")]
        public string BanReason;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt;

        [JsonProperty("banned_at")]
        public DateTime? BannedAt;

        [JsonProperty("banned_by")]
        public string BannedBy;
    }

    public class User
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("display_name")]
        public string DisplayName;

        [JsonProperty("avatar_url")]
        public string AvatarUrl;
    }
}
