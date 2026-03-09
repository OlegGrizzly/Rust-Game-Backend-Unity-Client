using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Transport.Interfaces;

namespace GameBackend.Transport.WebSocket
{
    /// <summary>
    /// IWebSocketAdapter implementation wrapping NativeWebSocket.
    /// NativeWebSocket requires calling DispatchMessageQueue() every frame in a MonoBehaviour Update().
    /// </summary>
    public class NativeWebSocketAdapter : IWebSocketAdapter
    {
        private NativeWebSocket.WebSocket _ws;

        public bool IsConnected => _ws != null && _ws.State == NativeWebSocket.WebSocketState.Open;

        public event Action OnOpen;
        public event Action<string> OnMessage;
        public event Action<string> OnError;
        public event Action OnClose;

        public async UniTask ConnectAsync(string url, CancellationToken ct)
        {
            _ws = new NativeWebSocket.WebSocket(url);

            _ws.OnOpen += () => OnOpen?.Invoke();
            _ws.OnMessage += bytes =>
            {
                var text = Encoding.UTF8.GetString(bytes);
                OnMessage?.Invoke(text);
            };
            _ws.OnError += error => OnError?.Invoke(error);
            _ws.OnClose += _ => OnClose?.Invoke();

            await _ws.Connect().AsUniTask().AttachExternalCancellation(ct);
        }

        public async UniTask SendAsync(string message, CancellationToken ct)
        {
            if (_ws == null || _ws.State != NativeWebSocket.WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not connected.");

            await _ws.SendText(message).AsUniTask().AttachExternalCancellation(ct);
        }

        public async UniTask CloseAsync()
        {
            if (_ws != null && _ws.State == NativeWebSocket.WebSocketState.Open)
            {
                await _ws.Close().AsUniTask();
            }
        }

        /// <summary>
        /// Must be called every frame (e.g., from MonoBehaviour.Update) to process incoming messages.
        /// NativeWebSocket queues messages internally and dispatches them only when this is called.
        /// </summary>
        public void DispatchMessageQueue()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _ws?.DispatchMessageQueue();
#endif
        }
    }
}
