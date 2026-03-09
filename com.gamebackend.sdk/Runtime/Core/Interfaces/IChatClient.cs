using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;
using Channel = GameBackend.Core.Models.Channel;

namespace GameBackend.Core.Interfaces
{
    public interface IChatClient
    {
        UniTask<IEnumerable<Channel>> ListChannelsAsync(CancellationToken ct = default);
        UniTask<IEnumerable<Channel>> ListRoomsAsync(int limit = 50, int offset = 0, CancellationToken ct = default);
        UniTask<Channel> CreateChannelAsync(CreateChannelRequest request, CancellationToken ct = default);
        UniTask<Channel> JoinRoomAsync(string channelId, CancellationToken ct = default);
        UniTask<MessageList> ListMessagesAsync(string channelId, int limit = 50, string cursor = null, string direction = "older", CancellationToken ct = default);
        UniTask<UnreadInfo> GetUnreadAsync(string channelId, CancellationToken ct = default);
        UniTask MarkChannelReadAsync(string channelId, CancellationToken ct = default);
    }
}
