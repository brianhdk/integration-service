using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Remote;

namespace Examples.LiteServer.Servers
{
    public class SlackHelloWorker : IBackgroundWorker
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SlackHelloWorker(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public BackgroundWorkerContinuation Work(CancellationToken token, BackgroundWorkerContext context)
        {
            using (HttpClient httpClient = _httpClientFactory.Create())
            {
                httpClient.BaseAddress = new Uri("https://slack.com/");

                string text = WebUtility.UrlEncode($"Hello from Integration Service @ {DateTimeOffset.Now.LocalDateTime}");

                httpClient
                    .GetAsync(
                        $"api/chat.postMessage?token=xoxb-63343022768-V6GIejWL2hIgM3dWkxt7tYS5&channel=%23automation&text={text}&as_user=true&pretty=1",
                        token)
                    .Wait(token);

                return context.Wait(TimeSpan.FromSeconds(10));
            }
        }
    }
}