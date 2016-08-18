using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SlackConnector;
using SlackConnector.Models;
using Vertica.Integration.Domain.LiteServer;

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
			return Task.Run(() =>
			{
				// https://api.slack.com/methods/chat.postMessage/test
				
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
}