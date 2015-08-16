using Microsoft.AspNet.SignalR;

namespace Integration.WebApi.SignalR.SmokeTester
{
	public class ChatHub : Hub
	{
		public void Send(string name, string message)
		{
			// Call the broadcastMessage method to update clients.
			Clients.All.broadcastMessage(name, message);
		}
	}
}