using System;

namespace GameBackend.Core.Exceptions
{
    public class GameApiException : Exception
    {
        /// <summary>HTTP status code</summary>
        public int StatusCode { get; }

        /// <summary>Error text from the server</summary>
        public string ErrorMessage { get; }

        /// <summary>Machine-readable error code (nullable, planned backend extension)</summary>
        public string ErrorCode { get; }

        /// <summary>Request ID for tracing (nullable, planned backend extension)</summary>
        public string RequestId { get; }

        public GameApiException(int statusCode, string errorMessage, string errorCode = null, string requestId = null)
            : base($"API error {statusCode}: {errorMessage}")
        {
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            RequestId = requestId;
        }
    }
}
