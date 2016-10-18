using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SlackConnector;
using SlackConnector.Models;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Remote;

namespace Vertica.Integration.Slack
{
	internal class SlackBackgroundServer : IBackgroundServer
	{
		private readonly TextWriter _writer;

		public SlackBackgroundServer(TextWriter writer)
		{
			_writer = writer;
		}

		public Task Create(CancellationToken token, BackgroundServerContext context)
		{
			// queue up messages/tasks to be run

			return Task.Run(() =>
			{
                // https://api.slack.com/methods/chat.postMessage/test


                // app client id: 40245908135.91750645699
                // app client secret: 27d2e7322071a18a02a3b6ec16eec99e

                SlackConnector.SlackConnector c = new SlackConnector.SlackConnector();
				ISlackConnection client = c.Connect("xoxb-63343022768-V6GIejWL2hIgM3dWkxt7tYS5").Result;

				client.OnMessageReceived += message =>
				{
					_writer.WriteLine(message.Text);
					
					var response = new BotMessage
					{
						Text = message.Text,
						ChatHub = message.ChatHub
					};

					return client.Say(response);
				};

				token.WaitHandle.WaitOne();

				client.Disconnect();

			}, token);
		}
	}

    internal class SlackWebApiServer : IBackgroundServer
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SlackWebApiServer(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task Create(CancellationToken token, BackgroundServerContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                using (HttpClient httpClient = _httpClientFactory.Create())
                {
                    httpClient.BaseAddress = new Uri("https://slack.com/");

                    while (!token.IsCancellationRequested)
                    {
                        string text = WebUtility.UrlEncode($"Sending a message: {DateTime.Now}");

                        httpClient
                            .GetAsync(
                                $"api/chat.postMessage?token=xoxb-63343022768-V6GIejWL2hIgM3dWkxt7tYS5&channel=%23automation&text={text}&as_user=true&pretty=1",
                                token);

                        token.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                    }
                }

            }, token);
        }
    }
}