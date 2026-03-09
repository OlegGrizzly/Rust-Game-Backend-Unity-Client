using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Interfaces;
using GameBackend.Core.Models;
using Channel = GameBackend.Core.Models.Channel;

namespace GameBackend.Api
{
    /// <summary>
    /// IChatClient implementation: channels, messages, unread counts.
    /// </summary>
    public class ChatService : IChatClient
    {
        private readonly HttpPipeline _pipeline;
        private readonly string _baseUrl;

        public ChatService(HttpPipeline pipeline, string baseUrl)
        {
            _pipeline = pipeline;
            _baseUrl = baseUrl;
        }

        public async UniTask<IEnumerable<Channel>> ListChannelsAsync(CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Url = $"{_baseUrl}/api/ws/channels"
            };
            return await _pipeline.SendAsync<List<Channel>>(request, ct);
        }

        public async UniTask<Channel> CreateChannelAsync(CreateChannelRequest createRequest, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "POST",
                Url = $"{_baseUrl}/api/ws/channels",
                Body = _pipeline.Serializer.Serialize(createRequest)
            };
            request.Headers["Content-Type"] = "application/json";
            return await _pipeline.SendAsync<Channel>(request, ct);
        }

        public async UniTask<MessageList> ListMessagesAsync(string channelId, int limit = 50,
            string cursor = null, string direction = "older", CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/api/ws/channels/{channelId}/messages?limit={limit}";
            if (cursor != null)
                url += $"&cursor={cursor}";
            if (direction != "older")
                url += $"&direction={direction}";

            var request = new HttpRequest
            {
                Method = "GET",
                Url = url
            };
            return await _pipeline.SendAsync<MessageList>(request, ct);
        }

        public async UniTask<UnreadInfo> GetUnreadAsync(string channelId, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Url = $"{_baseUrl}/api/ws/channels/{channelId}/unread"
            };
            return await _pipeline.SendAsync<UnreadInfo>(request, ct);
        }

        public async UniTask MarkChannelReadAsync(string channelId, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "POST",
                Url = $"{_baseUrl}/api/ws/channels/{channelId}/read"
            };
            await _pipeline.SendAsync(request, ct);
        }

        public async UniTask<IEnumerable<Channel>> ListRoomsAsync(int limit = 50, int offset = 0, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Url = $"{_baseUrl}/api/ws/channels/rooms?limit={limit}&offset={offset}"
            };
            return await _pipeline.SendAsync<List<Channel>>(request, ct);
        }

        public async UniTask<Channel> JoinRoomAsync(string channelId, CancellationToken ct = default)
        {
            var request = new HttpRequest
            {
                Method = "POST",
                Url = $"{_baseUrl}/api/ws/channels/{channelId}/join"
            };
            return await _pipeline.SendAsync<Channel>(request, ct);
        }
    }
}
