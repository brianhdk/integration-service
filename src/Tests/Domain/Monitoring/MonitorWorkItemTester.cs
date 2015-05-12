using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Tests.Domain.Monitoring
{
	[TestFixture]
	public class MonitorWorkItemTester
	{
		[Test]
		public void Add_VariousMessagesIncludingIgnoredOnes_VerifyCount()
		{
			var ignoreFilter1 = Substitute.For<ISpecification<MonitorEntry>>();
			var ignoreFilter2 = Substitute.For<ISpecification<MonitorEntry>>();

			ignoreFilter1.IsSatisfiedBy(Arg.Is<MonitorEntry>(entry => entry.Message == "Message to ignore")).Returns(true);
			ignoreFilter2.IsSatisfiedBy(Arg.Is<MonitorEntry>(entry => entry.Message == "Also to be ignored")).Returns(true);

		    var subject = new MonitorWorkItem(new MonitorConfiguration())
                .WithIgnoreFilter(ignoreFilter1)
                .WithIgnoreFilter(ignoreFilter2);

            subject.Add(Time.Now, "Source", "Not ignored message", Target.Service);
            subject.Add(Time.Now, "Source", "Message to ignore", Target.Service);
            subject.Add(Time.Now, "Source", "Also to be ignored", Target.Service);

		    MonitorEntry[] entries = subject.GetEntries(Target.Service);

            Assert.That(entries.Length, Is.EqualTo(1));
			Assert.That(entries[0].Message, Is.EqualTo("Not ignored message"));
		}

		[Test]
		public void Add_VariousMessagesNoFilters_VerifyCount()
		{
		    var subject = new MonitorWorkItem(new MonitorConfiguration());
            subject.Add(Time.Now, "Source", "Not ignored message", Target.Service);
            subject.Add(Time.Now, "Source", "Also not ignored", Target.Service);
            subject.Add(Time.Now, "Source", "And me neither", Target.Service);

            MonitorEntry[] entries = subject.GetEntries(Target.Service);

			Assert.That(entries.Length, Is.EqualTo(3));
		}
	}
}