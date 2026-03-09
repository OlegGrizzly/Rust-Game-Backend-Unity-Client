using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Exceptions;
using GameBackend.Core.Models;
using GameBackend.Transport.Interfaces;
using Newtonsoft.Json.Linq;

namespace GameBackend.Api
{
    /// <summary>
    /// HTTP request pipeline: adds auth headers, handles auto-refresh on 401,
    /// parses error responses, deserializes successful responses.
    /// </summary>
    public class HttpPipeline
    {
        private readonly IHttpAdapter _httpAdapter;
        private readonly ISerializer _serializer;
        private readonly TokenManager _tokenManager;
        private string _baseUrl;

        internal ISerializer Serializer => _serializer;

        public HttpPipeline(IHttpAdapter httpAdapter, ISerializer serializer, TokenManager tokenManager)
        {
            _httpAdapter = httpAdapter;
            _serializer = serializer;
            _tokenManager = tokenManager;
        }

        internal void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        /// <summary>Send request and deserialize response body to T.</summary>
        public async UniTask<T> SendAsync<T>(HttpRequest request, CancellationToken ct = default)
        {
            var response = await SendRawAsync(request, ct);
            return _serializer.Deserialize<T>(response.Body);
        }

        /// <summary>Send request without deserializing body (for DELETE 204 etc).</summary>
        public async UniTask SendAsync(HttpRequest request, CancellationToken ct = default)
        {
            await SendRawAsync(request, ct);
        }

        private async UniTask<HttpResponse> SendRawAsync(HttpRequest request, CancellationToken ct)
        {
            AddAuthHeader(request);
            var response = await _httpAdapter.SendAsync(request, ct);

            // Auto-refresh on 401
            if (response.StatusCode == 401 && _tokenManager.CurrentSession?.RefreshToken != null)
            {
                var refreshResult = await _tokenManager.RefreshAsync(_httpAdapter, _baseUrl, ct);
                if (refreshResult != null)
                {
                    // Retry original request with new token
                    AddAuthHeader(request);
                    response = await _httpAdapter.SendAsync(request, ct);
                }
                else
                {
                    // Refresh failed, tokens already cleared by TokenManager
                    ThrowApiException(response);
                }
            }

            if (response.StatusCode >= 400)
            {
                ThrowApiException(response);
            }

            return response;
        }

        private void AddAuthHeader(HttpRequest request)
        {
            var session = _tokenManager.CurrentSession;
            if (session != null)
            {
                request.Headers["Authorization"] = $"Bearer {session.AuthToken}";
            }
        }

        private static void ThrowApiException(HttpResponse response)
        {
            string errorMessage = "Unknown error";
            try
            {
                var errorObj = JObject.Parse(response.Body);
                errorMessage = errorObj.Value<string>("error") ?? errorMessage;
            }
            catch
            {
                // Body is not valid JSON or missing "error" field
            }

            throw new GameApiException(response.StatusCode, errorMessage);
        }
    }
}
