using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core;
using GameBackend.Core.Models;

namespace GameBackend.Api.Pipeline
{
    /// <summary>
    /// Exponential backoff retry middleware.
    /// Only for safe-to-retry (idempotent) operations.
    /// Auth operations (register, login) are NOT safe to retry.
    /// </summary>
    public static class RetryMiddleware
    {
        private static readonly Random Rng = new Random();

        /// <summary>
        /// Execute an HTTP operation with exponential backoff retry.
        /// </summary>
        public static async UniTask<HttpResponse> ExecuteWithRetryAsync(
            Func<UniTask<HttpResponse>> operation,
            RetryConfiguration config,
            CancellationToken ct = default)
        {
            HttpResponse lastResponse = null;

            for (int attempt = 0; attempt <= config.MaxRetries; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                lastResponse = await operation();

                if (lastResponse.StatusCode < 500 && lastResponse.StatusCode != 429)
                    return lastResponse;

                if (attempt >= config.MaxRetries)
                    break;

                config.RetryListener?.Invoke(attempt);

                int delayMs;
                if (lastResponse.StatusCode == 429 &&
                    lastResponse.Headers != null &&
                    lastResponse.Headers.TryGetValue("Retry-After", out var retryAfter) &&
                    int.TryParse(retryAfter, out var retryAfterSeconds))
                {
                    delayMs = retryAfterSeconds * 1000;
                }
                else
                {
                    // Exponential backoff: baseDelay * 2^attempt + jitter
                    delayMs = config.BaseDelayMs * (1 << attempt);
                    delayMs += Rng.Next(0, delayMs / 4 + 1);
                }

                await UniTask.Delay(delayMs, cancellationToken: ct);
            }

            return lastResponse;
        }
    }
}
