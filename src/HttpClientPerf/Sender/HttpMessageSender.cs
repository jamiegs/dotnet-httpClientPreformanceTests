using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientPerf.Sender
{
    public class SenderResult
    {
        public SenderResult(System.Net.HttpStatusCode statusCode, double requestMs)
        {
            StatusCode = statusCode;
            RequestMs = requestMs;
        }
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public double RequestMs { get; set; }
    }
    public class HttpMessageSender : IDisposable
    {
        private static HttpClient _httpClient;
        private readonly int _timeout;
        private readonly bool _disableKeepAlive;
        private readonly bool _alwaysCreateClient;

        public HttpMessageSender(bool disableKeepAlive, int timeout, bool alwaysCreateClient)
        {
            if (_httpClient != null)
            {
                return;
            }
            _disableKeepAlive = disableKeepAlive;
            _timeout = timeout;
            _alwaysCreateClient = alwaysCreateClient;
        }
        internal HttpClient CreateClient()
        {
            var httpClient = new HttpClient(); ;

            httpClient.Timeout = TimeSpan.FromMilliseconds(_timeout);

            if (_disableKeepAlive)
            {
                httpClient.DefaultRequestHeaders.Add("Connection", "close");
            }
            else
            {
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            }

            return httpClient;
        }
        internal HttpMessageSender(HttpClient client)
        {
            _httpClient = client;
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.CancelPendingRequests();
                _httpClient.Dispose();
            }
        }

        public async Task<SenderResult> Send(Uri uri, string message, IDictionary<string, string> headers, string contentType, CancellationToken cancellationToken)
        {
            var stringContent = new StringContent(message);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    stringContent.Headers.Add(header.Key, header.Value);
                }
            }

            HttpClient httpClient = null;
            if (_httpClient == null)
            {
                httpClient = CreateClient();
            }
            else
            {
                httpClient = _httpClient;
            }
            using (var l = new PerfTimerLogger("get request"))
            {
                var result = await httpClient.GetAsync(uri, cancellationToken);
                var resultStream = await result.Content.ReadAsStreamAsync();
                return new SenderResult(result.StatusCode, l.ElapsedMilliseconds);
            }
        }
    }
}