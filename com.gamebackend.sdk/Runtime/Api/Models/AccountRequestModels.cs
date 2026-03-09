using Newtonsoft.Json;

namespace GameBackend.Api.Models
{
    internal class UpdateAccountRequest
    {
        [JsonProperty("display_name", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("avatar_url", NullValueHandling = NullValueHandling.Ignore)]
        public string AvatarUrl { get; set; }

        [JsonProperty("lang", NullValueHandling = NullValueHandling.Ignore)]
        public string Lang { get; set; }

        [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
        public string Location { get; set; }

        [JsonProperty("timezone", NullValueHandling = NullValueHandling.Ignore)]
        public string Timezone { get; set; }
    }

    internal class BatchUserIdsRequest
    {
        [JsonProperty("user_ids")]
        public string[] UserIds { get; set; }
    }
}
