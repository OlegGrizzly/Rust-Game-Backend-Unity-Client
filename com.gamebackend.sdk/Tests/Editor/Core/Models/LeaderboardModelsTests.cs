using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using GameBackend.Core.Models;

namespace GameBackend.Tests.Core.Models
{
    [TestFixture]
    public class LeaderboardRecordTests
    {
        [Test]
        public void LeaderboardRecord_Deserialize_MapsAllFields()
        {
            var json = @"{
                ""leaderboard_id"": ""weekly_kills"",
                ""user_id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                ""score"": 2500,
                ""subscore"": 100,
                ""rank"": 1,
                ""metadata"": { ""level"": 10 },
                ""created_at"": ""2026-02-28T10:00:00Z"",
                ""updated_at"": ""2026-02-28T14:30:00Z""
            }";

            var result = JsonConvert.DeserializeObject<LeaderboardRecord>(json);

            Assert.AreEqual("weekly_kills", result.LeaderboardId);
            Assert.AreEqual(1, result.Rank);
            Assert.AreEqual("a1b2c3d4-e5f6-7890-abcd-ef1234567890", result.UserId);
            Assert.AreEqual(2500, result.Score);
            Assert.AreEqual(100, result.Subscore);
            Assert.IsNotNull(result.Metadata);
            Assert.AreEqual(new DateTime(2026, 2, 28, 10, 0, 0, DateTimeKind.Utc), result.CreatedAt);
            Assert.AreEqual(new DateTime(2026, 2, 28, 14, 30, 0, DateTimeKind.Utc), result.UpdatedAt);
        }

        [Test]
        public void LeaderboardRecord_Deserialize_NullMetadata_HandlesNull()
        {
            var json = @"{
                ""leaderboard_id"": ""weekly_kills"",
                ""user_id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                ""score"": 2500,
                ""subscore"": 100,
                ""rank"": 1,
                ""metadata"": null,
                ""created_at"": ""2026-02-28T10:00:00Z"",
                ""updated_at"": ""2026-02-28T14:30:00Z""
            }";

            var result = JsonConvert.DeserializeObject<LeaderboardRecord>(json);

            Assert.IsNull(result.Metadata);
        }
    }

    [TestFixture]
    public class LeaderboardRecordListTests
    {
        [Test]
        public void LeaderboardRecordList_Deserialize_WithRecords()
        {
            var json = @"{
                ""records"": [
                    {
                        ""leaderboard_id"": ""weekly_kills"",
                        ""user_id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                        ""score"": 2500,
                        ""subscore"": 100,
                        ""rank"": 1,
                        ""metadata"": null,
                        ""created_at"": ""2026-02-28T10:00:00Z"",
                        ""updated_at"": ""2026-02-28T14:30:00Z""
                    },
                    {
                        ""leaderboard_id"": ""weekly_kills"",
                        ""user_id"": ""b2c3d4e5-f6a7-8901-bcde-f12345678901"",
                        ""score"": 2100,
                        ""subscore"": 50,
                        ""rank"": 2,
                        ""metadata"": null,
                        ""created_at"": ""2026-02-28T11:00:00Z"",
                        ""updated_at"": ""2026-02-28T13:00:00Z""
                    }
                ]
            }";

            var result = JsonConvert.DeserializeObject<LeaderboardRecordList>(json);

            Assert.IsNotNull(result.Records);
            var records = result.Records.ToList();
            Assert.AreEqual(2, records.Count);
            Assert.AreEqual(1, records[0].Rank);
            Assert.AreEqual(2, records[1].Rank);
            Assert.AreEqual(2500, records[0].Score);
            Assert.AreEqual(2100, records[1].Score);
        }
    }
}
