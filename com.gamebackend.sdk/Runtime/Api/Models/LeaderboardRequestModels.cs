using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameBackend.Api.Models
{
    internal class WriteRecordRequest
    {
        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("subscore", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Subscore { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Metadata { get; set; }
    }

    internal class BatchRecordIdsRequest
    {
        [JsonProperty("user_ids")]
        public string[] UserIds { get; set; }
    }
}
