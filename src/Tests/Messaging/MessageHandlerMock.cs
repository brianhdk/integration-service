using Integration.Messaging;

namespace Vertica.Integration.Tests.Messaging
{
	public class TestMessage
	{
		
	}

	public class MessageHandlerMock : MessageHandler<TestMessage>
	{
		protected override void HandleMessage(TestMessage message)
		{
			throw new System.NotImplementedException();
		}
	}
}