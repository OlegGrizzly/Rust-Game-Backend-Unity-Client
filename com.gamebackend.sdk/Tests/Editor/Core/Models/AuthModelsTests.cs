using System;
using Newtonsoft.Json;
using NUnit.Framework;
using GameBackend.Core.Models;

namespace GameBackend.Tests.Core.Models
{
    [TestFixture]
    public class AuthResponseTests
    {
        [Test]
        public void AuthResponse_Deserialize_SnakeCaseJson_MapsAllFields()
        {
            var json = @"{
                ""access_token"": ""eyJhbGciOiJIUzI1NiJ9.access"",
                ""refresh_token"": ""eyJhbGciOiJIUzI1NiJ9.refresh"",
                ""expires_in"": 900,
                ""user"": {
                    ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                    ""username"": ""player1"",
                    ""display_name"": ""player1""
                }
            }";

            var result = JsonConvert.DeserializeObject<AuthResponse>(json);

            Assert.AreEqual("eyJhbGciOiJIUzI1NiJ9.access", result.AccessToken);
            Assert.AreEqual("eyJhbGciOiJIUzI1NiJ9.refresh", result.RefreshToken);
            Assert.AreEqual(900, result.ExpiresIn);
            Assert.IsNotNull(result.User);
        }

        [Test]
        public void AuthUser_Deserialize_MapsFields()
        {
            var json = @"{
                ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                ""username"": ""player1"",
                ""display_name"": ""Player One""
            }";

            var result = JsonConvert.DeserializeObject<AuthUser>(json);

            Assert.AreEqual("a1b2c3d4-e5f6-7890-abcd-ef1234567890", result.Id);
            Assert.AreEqual("player1", result.Username);
            Assert.AreEqual("Player One", result.DisplayName);
        }

        [Test]
        public void SessionInfo_Deserialize_MapsAllFields()
        {
            var json = @"{
                ""id"": ""b2c3d4e5-f6a7-8901-bcde-f12345678901"",
                ""device_info"": ""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"",
                ""ip_address"": ""85.158.109.109"",
                ""created_at"": ""2026-02-26T12:00:00Z"",
                ""expires_at"": ""2026-03-28T12:00:00Z""
            }";

            var result = JsonConvert.DeserializeObject<SessionInfo>(json);

            Assert.AreEqual("b2c3d4e5-f6a7-8901-bcde-f12345678901", result.Id);
            Assert.AreEqual("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36", result.DeviceInfo);
            Assert.AreEqual("85.158.109.109", result.IpAddress);
            Assert.AreEqual(new DateTime(2026, 2, 26, 12, 0, 0, DateTimeKind.Utc), result.CreatedAt);
            Assert.AreEqual(new DateTime(2026, 3, 28, 12, 0, 0, DateTimeKind.Utc), result.ExpiresAt);
        }
    }
}
