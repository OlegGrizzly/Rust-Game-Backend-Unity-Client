using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;
using GameBackend.Transport.Interfaces;

namespace GameBackend.Tests.Mocks
{
    public class MockHttpAdapter : IHttpAdapter
    {
        private readonly Queue<Func<HttpRequest, UniTask<HttpResponse>>> _responseQueue
            = new Queue<Func<HttpRequest, UniTask<HttpResponse>>>();

        public List<HttpRequest> SentRequests { get; } = new List<HttpRequest>();

        public void EnqueueResponse(HttpResponse response)
        {
            _responseQueue.Enqueue(_ => UniTask.FromResult(response));
        }

        public void EnqueueResponse(int statusCode, string body)
        {
            var response = new HttpResponse { StatusCode = statusCode, Body = body };
            _responseQueue.Enqueue(_ => UniTask.FromResult(response));
        }

        public void EnqueueException(Exception ex)
        {
            _responseQueue.Enqueue(_ => UniTask.FromException<HttpResponse>(ex));
        }

        public void EnqueueCallback(Func<HttpRequest, UniTask<HttpResponse>> callback)
        {
            _responseQueue.Enqueue(callback);
        }

        public UniTask<HttpResponse> SendAsync(HttpRequest request, CancellationToken ct)
        {
            SentRequests.Add(request);

            if (_responseQueue.Count == 0)
            {
                throw new InvalidOperationException(
                    $"MockHttpAdapter: no responses enqueued. Request: {request.Method} {request.Url}");
            }

            var handler = _responseQueue.Dequeue();
            return handler(request);
        }
    }
}
