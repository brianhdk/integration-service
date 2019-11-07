using NUnit.Framework;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Utilities;

namespace Vertica.Integration.Tests.Domain.Monitoring
{
	[TestFixture]
	public class MessageContainsTextTester
	{
		[Test]
		public void IsSatisfiedBy_MessageContainingText_True()
		{
			const string message = @"Sitecore.Exceptions.AccessDeniedException
Application access denied.

User: sitecore\Anonymous
Host: PETERJUSTESENWEB01
Details: /elmah.axd/detail?id=D1732EFF-3AA3-4A0E-A174-E51A2848522B";

			var entry = new MonitorEntry(Time.Now, "Source", message);

			var subject = new MessageContainsText(@"Sitecore.Exceptions.AccessDeniedException
Application access denied", "Unable to connect");

			bool actual = subject.IsSatisfiedBy(entry);

			Assert.That(actual, Is.True);
		}

		[Test]
		public void IsSatisfiedBy_MessageNotContainingText_False()
		{
			const string message = @"ServiceStack.Common.Web.HttpError
PDF file for invoice '3139004' not found on server. Customer: '3101152'.

User: some.user@domain.com
Host: PETERJUSTESENWEB01
Details: /elmah.axd/detail?id=4E6E10FF-3A3B-4C90-ADB4-FCAC3CEFBA33";

			var entry = new MonitorEntry(Time.Now, "Source", message);

			var subject = new MessageContainsText();

			bool actual = subject.IsSatisfiedBy(entry);

			Assert.That(actual, Is.False);
		}
	}
}