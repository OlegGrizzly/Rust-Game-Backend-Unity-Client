using System;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class AuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken;

        [JsonProperty("refresh_token")]
        public string RefreshToken;

        [JsonProperty("expires_in")]
        public int ExpiresIn;

        [JsonProperty("user")]
        public AuthUser User;
    }

    public class AuthUser
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("display_name")]
        public string DisplayName;
    }

    public class SessionInfo
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("device_info")]
        public string DeviceInfo;

        [JsonProperty("ip_address")]
        public string IpAddress;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;

        [JsonProperty("expires_at")]
        public DateTime ExpiresAt;
    }
}
