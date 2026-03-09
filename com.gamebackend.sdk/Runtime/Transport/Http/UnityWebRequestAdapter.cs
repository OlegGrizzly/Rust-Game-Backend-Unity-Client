using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using GameBackend.Core.Models;
using GameBackend.Transport.Interfaces;

namespace GameBackend.Transport.Http
{
    public class UnityWebRequestAdapter : IHttpAdapter
    {
        public async UniTask<HttpResponse> SendAsync(HttpRequest request, CancellationToken ct)
        {
            var webRequest = new UnityWebRequest(request.Url, request.Method);

            if (!string.IsNullOrEmpty(request.Body))
            {
                var bodyBytes = System.Text.Encoding.UTF8.GetBytes(request.Body);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyBytes);
            }

            webRequest.downloadHandler = new DownloadHandlerBuffer();

            foreach (var header in request.Headers)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }

            if (!string.IsNullOrEmpty(request.Body) && !request.Headers.ContainsKey("Content-Type"))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
            }

            try
            {
                await webRequest.SendWebRequest().ToUniTask(cancellationToken: ct);
            }
            catch (UnityWebRequestException)
            {
                // Do not throw — status code is already set on webRequest.
                // Error handling (4xx, 5xx) is responsibility of HttpPipeline.
            }

            var response = new HttpResponse
            {
                StatusCode = (int)webRequest.responseCode,
                Body = webRequest.downloadHandler?.text ?? string.Empty
            };

            var responseHeaders = webRequest.GetResponseHeaders();
            if (responseHeaders != null)
            {
                foreach (var header in responseHeaders)
                {
                    response.Headers[header.Key] = header.Value;
                }
            }

            webRequest.Dispose();

            return response;
        }
    }
}
