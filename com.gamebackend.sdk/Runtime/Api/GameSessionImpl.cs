using System;
using System.Text;
using GameBackend.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameBackend.Api
{
    /// <summary>
    /// IGameSession implementation that decodes JWT payload to extract claims.
    /// Access token is stored only in memory (short-lived, 15 min).
    /// </summary>
    public class GameSessionImpl : IGameSession
    {
        public string AuthToken { get; }
        public string RefreshToken { get; }
        public string UserId { get; }
        public string Username { get; }
        public string DisplayName { get; }
        public long ExpireTime { get; }
        public long RefreshExpireTime { get; }

        public bool IsExpired => HasExpired(DateTime.UtcNow);
        public bool IsRefreshExpired => HasRefreshExpired(DateTime.UtcNow);

        public GameSessionImpl(string authToken, string refreshToken)
        {
            AuthToken = authToken;
            RefreshToken = refreshToken;

            var accessPayload = DecodeJwtPayload(authToken);
            UserId = accessPayload.Value<string>("sub") ?? "";
            Username = accessPayload.Value<string>("username") ?? "";
            DisplayName = accessPayload.Value<string>("display_name") ?? "";
            ExpireTime = accessPayload.Value<long>("exp");

            var refreshPayload = DecodeJwtPayload(refreshToken);
            RefreshExpireTime = refreshPayload.Value<long>("exp");
        }

        public bool HasExpired(DateTime offset)
        {
            var unixTime = new DateTimeOffset(offset).ToUnixTimeSeconds();
            return unixTime >= ExpireTime;
        }

        public bool HasRefreshExpired(DateTime offset)
        {
            var unixTime = new DateTimeOffset(offset).ToUnixTimeSeconds();
            return unixTime >= RefreshExpireTime;
        }

        private static JObject DecodeJwtPayload(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2)
                throw new ArgumentException("Invalid JWT format");

            var payload = parts[1];
            // Base64url decode: replace URL-safe chars and add padding
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);
            return JObject.Parse(json);
        }
    }
}
