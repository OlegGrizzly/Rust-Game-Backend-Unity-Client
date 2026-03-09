using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class Group
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("avatar_url")]
        public string AvatarUrl;

        [JsonProperty("open")]
        public bool Open;

        [JsonProperty("max_count")]
        public int MaxCount;

        [JsonProperty("member_count")]
        public long MemberCount;

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata;

        [JsonProperty("channel_id")]
        public string ChannelId;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;
    }

    public class GroupMember
    {
        [JsonProperty("user_id")]
        public string UserId;

        [JsonProperty("role")]
        public string Role;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("display_name")]
        public string DisplayName;

        [JsonProperty("avatar_url")]
        public string AvatarUrl;

        [JsonProperty("joined_at")]
        public DateTime JoinedAt;
    }

    public class GroupJoinRequest
    {
        [JsonProperty("user_id")]
        public string UserId;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;
    }

    public class MyGroup
    {
        [JsonProperty("group")]
        public Group Group;

        [JsonProperty("role")]
        public string Role;
    }

    public class GroupEvent
    {
        [JsonProperty("group_id")]
        public string GroupId;

        [JsonProperty("group_name")]
        public string GroupName;

        [JsonProperty("user_id")]
        public string UserId;
    }
}
