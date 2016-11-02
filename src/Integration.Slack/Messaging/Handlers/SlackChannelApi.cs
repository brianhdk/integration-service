using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Slack.Messaging.Messages;

namespace Vertica.Integration.Slack.Messaging.Handlers
{
    internal class SlackChannelApi : 
        IHandleMessages<SlackPostMessageInChannel>
    {
        private readonly IHttpClientFactory _factory;

        public SlackChannelApi(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task Handle(SlackPostMessageInChannel message, CancellationToken token)
        {
            using (HttpClient httpClient = _factory.Create())
            {
                httpClient.BaseAddress = new Uri("https://slack.com/");

                string text = WebUtility.UrlEncode(message.Text);

                var url = $"api/chat.postMessage?token=xoxb-63343022768-V6GIejWL2hIgM3dWkxt7tYS5&channel=%23automation&text={text}&as_user=true&pretty=1";

                var response = await httpClient.GetAsync(url, token);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}