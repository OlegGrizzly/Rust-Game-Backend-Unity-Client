using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Core.Interfaces
{
    public interface IStorageClient
    {
        UniTask WriteStorageObjectsAsync(IEnumerable<StorageObjectWrite> objects, CancellationToken ct = default);
        UniTask<IEnumerable<StorageObject>> ReadStorageObjectsAsync(IEnumerable<StorageObjectId> keys, CancellationToken ct = default);
        UniTask DeleteStorageObjectAsync(string collection, string key, CancellationToken ct = default);
        UniTask<IEnumerable<StorageObject>> SearchStorageObjectsAsync(string collection, string query, CancellationToken ct = default);
        UniTask<int> CountStorageObjectsAsync(string collection, string key = null, CancellationToken ct = default);
    }
}
