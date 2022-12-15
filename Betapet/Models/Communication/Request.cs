using System.Collections.ObjectModel;

namespace Betapet.Models.Communication
{
    public class Request
    {
        private static readonly List<RequestHeader> _defaultHeaders = new()
        {
            new RequestHeader("Host", "mobile-app-se.betapet.se"),
            new RequestHeader("Accept", "*/*"),
            new RequestHeader("Accept-Language", "sv-SE,sv;q=0.9"),
            new RequestHeader("Connection", "keep-alive"),
            new RequestHeader("Accept-Encoding", "gzip, deflate, br"),
            new RequestHeader("User-Agent", "Betapet/1.71 CFNetwork/1399 Darwin/22.1.0"),
        };

        private static ReadOnlyCollection<RequestHeader>? defaultHeaders;

        public static ReadOnlyCollection<RequestHeader> DefaultHeaders
        {
            get
            {
                if (defaultHeaders == null)
                    defaultHeaders = new ReadOnlyCollection<RequestHeader>(_defaultHeaders);

                return defaultHeaders;
            }
        }

        public string Host { get; set; }
        public string Path { get; set; }
        public List<QueryParameter> Parameters { get; set; }
        public HttpMethod Method { get; set; }
        public List<RequestHeader> Headers { get; set; }

        public Request(string host, string path, HttpMethod method)
        {
            Host = host;
            Path = path;
            Method = method;
            Headers = new List<RequestHeader>();
            Parameters = new List<QueryParameter>();

            foreach (RequestHeader header in DefaultHeaders)
                Headers.Add(header);
        }

        public void AddParameter(QueryParameter paramter)
        {
            Parameters.Add(paramter);
        }

        public void AddParameter(string name, string value)
        {
            Parameters.Add(new QueryParameter(name, value));
        }
    }
}
