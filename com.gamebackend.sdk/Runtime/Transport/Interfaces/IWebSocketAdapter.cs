using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameBackend.Transport.Interfaces
{
    /// <summary>WebSocket transport abstraction.</summary>
    public interface IWebSocketAdapter
    {
        bool IsConnected { get; }
        event Action OnOpen;
        event Action<string> OnMessage;
        event Action<string> OnError;
        event Action OnClose;
        UniTask ConnectAsync(string url, CancellationToken ct);
        UniTask SendAsync(string message, CancellationToken ct);
        UniTask CloseAsync();
    }
}
