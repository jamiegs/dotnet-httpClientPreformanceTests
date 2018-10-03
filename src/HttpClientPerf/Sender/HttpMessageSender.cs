using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientPerf.Sender

{
    public class HttpMessageSender : IDisposable
    {
        private static HttpClient _httpClient;
        private readonly int _timeout;
        private readonly bool _disableKeepAlive;
        private readonly bool _alwaysCreateClient;

        public HttpMessageSender(bool disableKeepAlive, int timeout,bool alwaysCreateClient)
        {
            if (_httpClient != null)
            {
                return;
            }
            _timeout = timeout;
            _alwaysCreateClient = alwaysCreateClient;
        }
        internal HttpClient CreateClient(){
            var httpClient = new HttpClient();;           
         
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

        public async Task<System.Net.HttpStatusCode> Send(Uri uri, string message, IDictionary<string, string> headers, string contentType, CancellationToken cancellationToken)
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
            if(_httpClient == null)
            {
                httpClient = CreateClient();
            }
            else
            {
                httpClient = _httpClient;
            }

            var result = await httpClient.GetAsync(uri, cancellationToken);

            var resultStreamTask = result.Content.ReadAsStreamAsync();
            var resultStream = await resultStreamTask;
            return result.StatusCode;
        }
    }
}