using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Slack.Messaging.Messages;

namespace Vertica.Integration.Slack.Messaging.Handlers
{
    internal class SlackChannelApi : 
        IHandleMessages<SlackPostMessageInChannel>,
        IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ISlackConfiguration _configuration;

        public SlackChannelApi(ISlackConfiguration configuration)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://slack.com/")
            };

            _configuration = configuration;
        }

        public async Task Handle(SlackPostMessageInChannel message, CancellationToken token)
        {
            string botUserToken = WebUtility.UrlEncode(_configuration.BotUserToken);
            string channel = WebUtility.UrlEncode(_configuration.DefaultChannel);
            string text = WebUtility.UrlEncode(message.Text);

            var url = $"api/chat.postMessage?token={botUserToken}&channel={channel}&text={text}&as_user=true&pretty=1";

            HttpResponseMessage response = await _httpClient.GetAsync(url, token);

            response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}