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

		public ChatHub(RandomChatter chatter, string s = null)
		{
			if (chatter == null) throw new ArgumentNullException("chatter");

			_chatter = chatter;
		}

		//public override Task OnConnected()
		//{
		//	_chatter.StartIfNotStarted();

		//	return base.OnConnected();
		//}

		public void Send(string name, string message)
		{
			if (String.Equals(message, "invalid", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("asdf");

			if (String.Equals(message, "argument", StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException(@"Invalid name", "name");

			if (String.Equals(message, "httpexception", StringComparison.OrdinalIgnoreCase))
				throw new HttpResponseException(HttpStatusCode.BadRequest);

			if (String.Equals(message, "start"))
				_chatter.StartIfNotStarted();

			if (String.Equals(message, "stop"))
				_chatter.Stop();

			// Call the broadcastMessage method to update clients.
			Clients.All.broadcastMessage(name, message);
		}

		public class RandomChatter
		{
			private readonly ManualResetEvent _waitHandle;

			public RandomChatter()
			{
				_waitHandle = new ManualResetEvent(false);
			}

			public void StartIfNotStarted()
			{
				if (Started)
					return;

				Action<string> write = msg => 
					GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.All.broadCastMessage("Random", msg);

				Started = true;

				Task.Run(() =>
				{
					while (Started)
					{
						write(Guid.NewGuid().ToString("N"));
						_waitHandle.WaitOne(TimeSpan.FromSeconds(10));
					}

					write("Stopped");
					_waitHandle.Reset();
				});
			}

			public void Stop()
			{
				Started = false;
				_waitHandle.Set();
			}

			private bool Started { get; set; }
		}
	}
}