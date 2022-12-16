using Betapet.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Helpers
{
    public class ApiHelper
    {
        private HttpClient httpClient;

        public ApiHelper()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            httpClient = new HttpClient(handler);

            foreach (RequestHeader header in Request.DefaultHeaders)
            {
                httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);
            }
        }

        public async Task<HttpResponseMessage> GetResponseAsync(Request request)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage()
            {
                Method = request.Method,
                RequestUri = request.GetUri(),
            };

            return await httpClient.SendAsync(httpRequest);
        }
    }
}
