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
        private readonly ISlackConfiguration _configuration;

        public SlackChannelApi(IHttpClientFactory factory, ISlackConfiguration configuration)
        {
            _factory = factory;
            _configuration = configuration;
        }

        public async Task Handle(SlackPostMessageInChannel message, CancellationToken token)
        {
            using (HttpClient httpClient = _factory.Create())
            {
                httpClient.BaseAddress = new Uri("https://slack.com/");

                string botUserToken = WebUtility.UrlEncode(_configuration.BotUserToken);
                string channel = WebUtility.UrlEncode(_configuration.DefaultChannel);
                string text = WebUtility.UrlEncode(message.Text);

                var url = $"api/chat.postMessage?token={botUserToken}&channel={channel}&text={text}&as_user=true&pretty=1";

                HttpResponseMessage response = await httpClient.GetAsync(url, token);

                response.EnsureSuccessStatusCode();
            }
        }
    }
}