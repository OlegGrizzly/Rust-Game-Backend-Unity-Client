using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Transport.Interfaces;

namespace GameBackend.Tests.Mocks
{
    public class MockWebSocketAdapter : IWebSocketAdapter
    {
        public bool IsConnected { get; private set; }
        public string LastConnectUrl { get; private set; }
        public List<string> SentMessages { get; } = new List<string>();

        public event Action OnOpen;
        public event Action<string> OnMessage;
        public event Action<string> OnError;
        public event Action OnClose;

        // If true, ConnectAsync will simulate immediate connection (calls OnOpen)
        public bool AutoOpen { get; set; } = true;

        // If set, ConnectAsync will throw this exception
        public Exception ConnectException { get; set; }

        public UniTask ConnectAsync(string url, CancellationToken ct)
        {
            LastConnectUrl = url;

            if (ConnectException != null)
            {
                return UniTask.FromException(ConnectException);
            }

            IsConnected = true;

            if (AutoOpen)
            {
                OnOpen?.Invoke();
            }

            return UniTask.CompletedTask;
        }

        public UniTask SendAsync(string message, CancellationToken ct)
        {
            SentMessages.Add(message);
            return UniTask.CompletedTask;
        }

        public UniTask CloseAsync()
        {
            IsConnected = false;
            return UniTask.CompletedTask;
        }

        // === Test helpers ===

        /// <summary>Simulate receiving a message from the server.</summary>
        public void SimulateMessage(string json)
        {
            OnMessage?.Invoke(json);
        }

        /// <summary>Simulate the connection opening.</summary>
        public void SimulateOpen()
        {
            IsConnected = true;
            OnOpen?.Invoke();
        }

        /// <summary>Simulate the connection closing unexpectedly.</summary>
        public void SimulateClose()
        {
            IsConnected = false;
            OnClose?.Invoke();
        }

        /// <summary>Simulate a WebSocket error.</summary>
        public void SimulateError(string error)
        {
            OnError?.Invoke(error);
        }
    }
}
