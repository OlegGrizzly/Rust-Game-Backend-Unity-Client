using System;

namespace GameBackend.Core
{
    public class RetryConfiguration
    {
        /// <summary>Base delay in ms (raised to power of attempt number)</summary>
        public int BaseDelayMs { get; }

        /// <summary>Maximum number of retries</summary>
        public int MaxRetries { get; }

        /// <summary>Callback before each retry attempt (optional)</summary>
        public Action<int> RetryListener { get; }

        public RetryConfiguration(int baseDelayMs, int maxRetries, Action<int> listener = null)
        {
            BaseDelayMs = baseDelayMs;
            MaxRetries = maxRetries;
            RetryListener = listener;
        }
    }
}
