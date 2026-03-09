using System;
using System.Text;

namespace GameBackend.Tests.Mocks
{
    /// <summary>
    /// Creates minimal JWT tokens for testing purposes.
    /// Produces tokens with base64url-encoded header and payload (no padding).
    /// The signature is a dummy value — these tokens are NOT cryptographically valid.
    /// </summary>
    public static class TestJwtHelper
    {
        public const string DefaultUserId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
        public const string DefaultUsername = "player1";
        public const string DefaultDisplayName = "Player One";

        /// <summary>Creates a test JWT with the given claims.</summary>
        public static string CreateTestJwt(
            string userId,
            string username,
            string displayName,
            long expUnix)
        {
            var header = Base64UrlEncode("{\"alg\":\"HS256\",\"typ\":\"JWT\"}");
            var payload = Base64UrlEncode(
                $"{{\"sub\":\"{userId}\",\"username\":\"{username}\",\"display_name\":\"{displayName}\",\"exp\":{expUnix}}}");
            return $"{header}.{payload}.test_signature";
        }

        /// <summary>Creates an access token expiring far in the future.</summary>
        public static string CreateValidAccessToken(
            string userId = DefaultUserId,
            string username = DefaultUsername,
            string displayName = DefaultDisplayName)
        {
            return CreateTestJwt(userId, username, displayName, 9999999999L);
        }

        /// <summary>Creates an access token that is already expired.</summary>
        public static string CreateExpiredAccessToken(
            string userId = DefaultUserId,
            string username = DefaultUsername,
            string displayName = DefaultDisplayName)
        {
            return CreateTestJwt(userId, username, displayName, 1000000000L);
        }

        /// <summary>Creates a refresh token (same structure, different exp).</summary>
        public static string CreateValidRefreshToken(
            string userId = DefaultUserId,
            string username = DefaultUsername,
            string displayName = DefaultDisplayName)
        {
            return CreateTestJwt(userId, username, displayName, 9999999999L);
        }

        /// <summary>
        /// Builds a realistic auth response JSON matching the backend format.
        /// </summary>
        public static string BuildAuthResponseJson(
            string accessToken = null,
            string refreshToken = null,
            int expiresIn = 900,
            string userId = DefaultUserId,
            string username = DefaultUsername,
            string displayName = DefaultDisplayName)
        {
            accessToken = accessToken ?? CreateValidAccessToken(userId, username, displayName);
            refreshToken = refreshToken ?? CreateValidRefreshToken(userId, username, displayName);

            return $"{{\"access_token\":\"{accessToken}\","
                 + $"\"refresh_token\":\"{refreshToken}\","
                 + $"\"expires_in\":{expiresIn},"
                 + $"\"user\":{{\"id\":\"{userId}\",\"username\":\"{username}\",\"display_name\":\"{displayName}\"}}}}";
        }

        /// <summary>Builds an error response JSON matching the backend format.</summary>
        public static string BuildErrorJson(string errorMessage)
        {
            return $"{{\"error\":\"{errorMessage}\"}}";
        }

        private static string Base64UrlEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}
