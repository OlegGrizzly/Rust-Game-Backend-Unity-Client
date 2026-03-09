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
    }

    public class ChatMessage
    {
        [JsonProperty("channel_id")]
        public string ChannelId;

        [JsonProperty("sender_id")]
        public string SenderId;

        [JsonProperty("sender_username")]
        public string SenderUsername;

        [JsonProperty("content")]
        public string Content;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;
    }

    public class MessageList
    {
        [JsonProperty("messages")]
        public IReadOnlyList<ChatMessage> Messages;

        [JsonProperty("next_cursor")]
        public string NextCursor;
    }

    public class CreateChannelRequest
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("channel_type")]
        public string ChannelType;

        [JsonProperty("target_user_id")]
        public string TargetUserId;
    }

    public class UnreadInfo
    {
        [JsonProperty("count")]
        public int Count;
    }
}
