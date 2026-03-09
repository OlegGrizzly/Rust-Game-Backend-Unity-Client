using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Api.Models;
using GameBackend.Core.Interfaces;
using GameBackend.Core.Models;

namespace GameBackend.Api
{
    /// <summary>
    /// IAccountClient implementation: profile CRUD, batch user lookup.
    /// </summary>
    public class AccountService : IAccountClient
    {
        private readonly HttpPipeline _pipeline;
        private readonly string _baseUrl;

        public AccountService(HttpPipeline pipeline, string baseUrl)
        {
            _pipeline = pipeline;
            _baseUrl = baseUrl;
        }

        public async UniTask<Account> GetAccountAsync(CancellationToken ct = default)
        {
            var request = new HttpRequest { Method = "GET", Url = $"{_baseUrl}/api/account/me" };
            return await _pipeline.SendAsync<Account>(request, ct);
        }

        public async UniTask UpdateAccountAsync(string displayName = null, string avatarUrl = null,
            string lang = null, string location = null, string timezone = null, CancellationToken ct = default)
        {
            var body = new UpdateAccountRequest
            {
                DisplayName = displayName,
                AvatarUrl = avatarUrl,
                Lang = lang,
                Location = location,
                Timezone = timezone
            };
            var request = new HttpRequest
            {
                Method = "PUT",
                Url = $"{_baseUrl}/api/account/me",
                Body = _pipeline.Serializer.Serialize(body)
            };
            request.Headers["Content-Type"] = "application/json";
            await _pipeline.SendAsync(request, ct);
        }

        public async UniTask DeleteAccountAsync(CancellationToken ct = default)
        {
            var request = new HttpRequest { Method = "DELETE", Url = $"{_baseUrl}/api/account/me" };
            await _pipeline.SendAsync(request, ct);
        }

        public async UniTask<User> GetUserAsync(string userId, CancellationToken ct = default)
        {
            var request = new HttpRequest { Method = "GET", Url = $"{_baseUrl}/api/account/{userId}" };
            return await _pipeline.SendAsync<User>(request, ct);
        }

        public async UniTask<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userIds, CancellationToken ct = default)
        {
            var body = new BatchUserIdsRequest { UserIds = userIds.ToArray() };
            var request = new HttpRequest
            {
                Method = "POST",
                Url = $"{_baseUrl}/api/account/batch",
                Body = _pipeline.Serializer.Serialize(body)
            };
            request.Headers["Content-Type"] = "application/json";
            return await _pipeline.SendAsync<List<User>>(request, ct);
        }
    }
}
