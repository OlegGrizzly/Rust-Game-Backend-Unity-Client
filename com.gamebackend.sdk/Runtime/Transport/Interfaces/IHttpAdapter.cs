using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Transport.Interfaces
{
    /// <summary>HTTP transport abstraction. Replaceable for tests.</summary>
    public interface IHttpAdapter
    {
        UniTask<HttpResponse> SendAsync(HttpRequest request, CancellationToken ct);
    }
}
