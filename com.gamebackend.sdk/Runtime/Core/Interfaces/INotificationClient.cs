using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Core.Interfaces
{
    public interface INotificationClient
    {
        UniTask<NotificationList> ListNotificationsAsync(int start = 0, int end = 25, bool unreadOnly = false, CancellationToken ct = default);
        UniTask<int> GetUnreadNotificationCountAsync(CancellationToken ct = default);
        UniTask<int> MarkNotificationsReadAsync(IEnumerable<string> ids, CancellationToken ct = default);
        UniTask<int> DeleteNotificationsAsync(IEnumerable<string> ids, CancellationToken ct = default);
    }
}
