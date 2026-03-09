using System;
using Newtonsoft.Json;
using NUnit.Framework;
using GameBackend.Core.Models;

namespace GameBackend.Tests.Core.Models
{
    [TestFixture]
    public class AccountTests
    {
        [Test]
        public void Account_Deserialize_AllFields_MapsCorrectly()
        {
            var json = @"{
                ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                ""username"": ""player1"",
                ""display_name"": ""Player One"",
                ""avatar_url"": ""https://example.com/avatar.png"",
                ""lang"": ""en"",
                ""location"": ""Moscow"",
                ""timezone"": ""Europe/Moscow"",
                ""is_banned"": false,
                ""ban_reason"": null,
                ""created_at"": ""2026-02-26T12:00:00Z"",
                ""updated_at"": ""2026-02-28T10:00:00Z"",
                ""banned_at"": null,
                ""banned_by"": null
            }";

            var result = JsonConvert.DeserializeObject<Account>(json);

            Assert.AreEqual("a1b2c3d4-e5f6-7890-abcd-ef1234567890", result.Id);
            Assert.AreEqual("player1", result.Username);
            Assert.AreEqual("Player One", result.DisplayName);
            Assert.AreEqual("https://example.com/avatar.png", result.AvatarUrl);
            Assert.AreEqual("en", result.Lang);
            Assert.AreEqual("Moscow", result.Location);
            Assert.AreEqual("Europe/Moscow", result.Timezone);
            Assert.IsFalse(result.IsBanned);
            Assert.IsNull(result.BanReason);
            Assert.AreEqual(new DateTime(2026, 2, 26, 12, 0, 0, DateTimeKind.Utc), result.CreatedAt);
            Assert.AreEqual(new DateTime(2026, 2, 28, 10, 0, 0, DateTimeKind.Utc), result.UpdatedAt);
            Assert.IsNull(result.BannedAt);
            Assert.IsNull(result.BannedBy);
        }

        [Test]
        public void Account_Deserialize_NullableFields_HandlesNull()
        {
            var json = @"{
                ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                ""username"": ""player1"",
                ""display_name"": ""player1"",
                ""avatar_url"": null,
                ""lang"": ""en"",
                ""location"": """",
                ""timezone"": """",
                ""is_banned"": false,
                ""ban_reason"": null,
                ""created_at"": ""2026-02-26T12:00:00Z"",
                ""updated_at"": null,
                ""banned_at"": null,
                ""banned_by"": null
            }";

            var result = JsonConvert.DeserializeObject<Account>(json);

            Assert.IsNull(result.AvatarUrl);
            Assert.IsNull(result.UpdatedAt);
        }
    }

    [TestFixture]
    public class UserTests
    {
        [Test]
        public void User_Deserialize_MapsFields()
        {
            var json = @"{
                ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                ""username"": ""player1"",
                ""display_name"": ""Player One"",
                ""avatar_url"": ""https://example.com/avatar.png""
            }";

            var result = JsonConvert.DeserializeObject<User>(json);

            Assert.AreEqual("a1b2c3d4-e5f6-7890-abcd-ef1234567890", result.Id);
            Assert.AreEqual("player1", result.Username);
            Assert.AreEqual("Player One", result.DisplayName);
            Assert.AreEqual("https://example.com/avatar.png", result.AvatarUrl);
        }
    }
}
