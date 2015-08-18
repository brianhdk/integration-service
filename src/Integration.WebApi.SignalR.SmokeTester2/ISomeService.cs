using System.Collections.Generic;

namespace Integration.WebApi.SignalR.SmokeTester2
{
	public interface ISomeService
	{
		void Say(string message);
		IEnumerable<string> Messages { get; }
	}

	public class SomeService : ISomeService
	{
		private readonly List<string> _messages;

		public SomeService()
		{
			_messages = new List<string>();
		}

		public void Say(string message)
		{
			_messages.Add(message);
		}

		public IEnumerable<string> Messages
		{
			get { return _messages; }
		}
	}
}