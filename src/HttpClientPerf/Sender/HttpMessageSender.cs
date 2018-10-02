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
        public HttpMessageSender(bool disableKeepAlive, int timeout)
        {
            // Create _httpClient once only
            if (_httpClient != null)
            {
                return;
            }

            _httpClient = new HttpClient(new HttpClientHandler
            {
                UseProxy = false
            });
            
            if (timeout > 0) _httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
           
            if (disableKeepAlive)
            {
                _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            }
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
            _httpClient.DefaultRequestHeaders.Add("Connection", "close");

            //var result = await _httpClient.PostAsync(uri, stringContent, cancellationToken);
            var result = await _httpClient.GetAsync(uri, cancellationToken);

            var resultStreamTask = result.Content.ReadAsStreamAsync();
            var resultStream = await resultStreamTask;

            return result.StatusCode;
        }
    }
}