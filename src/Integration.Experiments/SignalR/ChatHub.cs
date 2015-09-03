using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Vertica.Integration.Experiments.SignalR
{
	public class ChatHub : Hub
	{
		private readonly Func<RandomChatter> _chatter;

		public ChatHub(Func<RandomChatter> chatter)
		{
			_chatter = chatter;
		}

		public override Task OnConnected()
		{
			_chatter();

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
			public RandomChatter()
			{
				IHubConnectionContext<dynamic> clients = GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients;

				Task.Run(() =>
				{
					for (int i = 0; i < 20; i++)
					{
						clients.All.broadCastMessage("Random", Guid.NewGuid().ToString("N"));
						Thread.Sleep(1000);
					}
				});
			}
		}
	}
}