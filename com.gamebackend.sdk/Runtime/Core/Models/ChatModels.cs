using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class Channel
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("channel_type")]
        public string ChannelType;

        [JsonProperty("context")]
        public string Context;

        [JsonProperty("created_by")]
        public string CreatedBy;

        [JsonProperty("created_at")]
        public string CreatedAt;
    }

    public class ChatMessage
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("channel_id")]
        public string ChannelId;

        [JsonProperty("sender_id")]
        public string SenderId;

        [JsonProperty("sender_name")]
        public string SenderName;

        [JsonProperty("content")]
        public string Content;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;

        [JsonProperty("updated_at")]
        public string UpdatedAt;

        [JsonProperty("deleted_at")]
        public string DeletedAt;
    }

    public class MessageList
    {
        [JsonProperty("messages")]
        public IReadOnlyList<ChatMessage> Messages;

        [JsonProperty("next_cursor")]
        public string NextCursor;

        [JsonProperty("prev_cursor")]
        public string PrevCursor;

        [JsonProperty("has_more")]
        public bool HasMore;
    }

    public class CreateChannelRequest
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("channel_type")]
        public string ChannelType;

        [JsonProperty("member_ids")]
        public string[] MemberIds;
    }

    public class UnreadInfo
    {
        [JsonProperty("unread_count")]
        public int UnreadCount;
    }
}
