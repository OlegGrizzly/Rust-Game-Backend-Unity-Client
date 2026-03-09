using Newtonsoft.Json;

namespace GameBackend.Api.Models
{
    internal class RegisterUsernameRequest
    {
        [JsonProperty("provider")] public string Provider = "username";
        [JsonProperty("username")] public string Username;
        [JsonProperty("password")] public string Password;
    }

    internal class RegisterEmailRequest
    {
        [JsonProperty("provider")] public string Provider = "email";
        [JsonProperty("email")] public string Email;
        [JsonProperty("password")] public string Password;
        [JsonProperty("username")] public string Username;
    }

    internal class RegisterDeviceRequest
    {
        [JsonProperty("provider")] public string Provider = "device";
        [JsonProperty("device_id")] public string DeviceId;
    }

    internal class LoginUsernameRequest
    {
        [JsonProperty("provider")] public string Provider = "username";
        [JsonProperty("username")] public string Username;
        [JsonProperty("password")] public string Password;
    }

    internal class LoginEmailRequest
    {
        [JsonProperty("provider")] public string Provider = "email";
        [JsonProperty("email")] public string Email;
        [JsonProperty("password")] public string Password;
    }

    internal class LoginDeviceRequest
    {
        [JsonProperty("provider")] public string Provider = "device";
        [JsonProperty("device_id")] public string DeviceId;
    }

    internal class LinkProviderRequest
    {
        [JsonProperty("provider")] public string Provider;
        [JsonProperty("provider_id")] public string ProviderId;
        [JsonProperty("secret")] public string Secret;
    }
}
