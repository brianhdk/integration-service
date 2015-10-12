using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.SignalR;

namespace Vertica.Integration.Experiments.SignalR
{
	public class ChatHub : Hub
	{
		private readonly RandomChatter _chatter;

		public ChatHub(RandomChatter chatter)
		{
			if (chatter == null) throw new ArgumentNullException("chatter");

			_chatter = chatter;
		}

		public override Task OnConnected()
		{
			_chatter.Start();

			return base.OnConnected();
		}

		public void Send(string name, string message)
		{
			if (String.Equals(message, "invalid", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("asdf");

			if (String.Equals(message, "argument", StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException(@"Invalid name", "name");

			if (String.Equals(message, "httpexception", StringComparison.OrdinalIgnoreCase))
				throw new HttpResponseException(HttpStatusCode.BadRequest);

			// Call the broadcastMessage method to update clients.
			Clients.All.broadcastMessage(name, message);
		}

		public class RandomChatter
		{
			public void Start()
			{
				if (Started)
					return;

				Started = true;

				Task.Run(() =>
				{
					for (int i = 0; i < 20; i++)
					{
						GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.All.broadCastMessage("Random", Guid.NewGuid().ToString("N"));
						Thread.Sleep(1000);
					}
				});
			}

			private bool Started { get; set; }
		}
	}
}