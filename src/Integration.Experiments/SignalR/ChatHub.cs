using System;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.SignalR;

namespace Vertica.Integration.Experiments.SignalR
{
	public class ChatHub : Hub
	{
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
	}
}