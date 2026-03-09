using System;
using Newtonsoft.Json;

namespace GameBackend.Core.Models
{
    public class StorageObject
    {
        [JsonProperty("collection")]
        public string Collection;

        [JsonProperty("key")]
        public string Key;

        [JsonProperty("user_id")]
        public string UserId;

        [JsonProperty("value")]
        public string Value;

        [JsonProperty("version")]
        public string Version;

        [JsonProperty("read_perm")]
        public int ReadPerm;

        [JsonProperty("write_perm")]
        public int WritePerm;

        [JsonProperty("created_at")]
        public DateTime CreatedAt;

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt;
    }

    public class StorageObjectWrite
    {
        [JsonProperty("collection")]
        public string Collection;

        [JsonProperty("key")]
        public string Key;

        [JsonProperty("value")]
        public string Value;

        [JsonProperty("read_perm")]
        public int? ReadPerm;

        [JsonProperty("write_perm")]
        public int? WritePerm;

        [JsonProperty("version")]
        public string Version;
    }

    public class StorageObjectId
    {
        [JsonProperty("collection")]
        public string Collection;

        [JsonProperty("key")]
        public string Key;

        [JsonProperty("user_id")]
        public string UserId;
    }
}
