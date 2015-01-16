using NUnit.Framework;

namespace Vertica.Integration.Tests.Messaging
{
	[TestFixture]
	public class MessageHandlerTests
	{
		[Test]
		public void CanHandle_ValidMessage_ReturnsTrue()
		{
			var subject = new MessageHandlerMock();
			
		}
	}
}