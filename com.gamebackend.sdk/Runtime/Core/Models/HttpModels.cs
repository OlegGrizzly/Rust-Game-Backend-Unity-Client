using System.Collections.Generic;

namespace GameBackend.Core.Models
{
    public class HttpRequest
    {
        public string Method;
        public string Url;
        public Dictionary<string, string> Headers;
        public string Body;

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }
    }

    public class HttpResponse
    {
        public int StatusCode;
        public Dictionary<string, string> Headers;
        public string Body;

        public HttpResponse()
        {
            Headers = new Dictionary<string, string>();
        }
    }
}
