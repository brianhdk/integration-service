using Microsoft.AspNet.SignalR;

namespace Integration.WebApi.SignalR.SmokeTester2
{
	public class ChatHub : Hub
	{
		private readonly ISomeService _service;

		public ChatHub(ISomeService service)
		{
			_service = service;
		}

		public void Send(string name, string message)
		{
			//_service.Say(message);

			// Call the broadcastMessage method to update clients.
			Clients.All.broadcastMessage(name, message);
		}
	}
}