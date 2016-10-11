using System;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Patterns;
using Vertica.Utilities_v4.Testing;

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
                .AddIgnoreFilter(ignoreFilter1)
                .AddIgnoreFilter(ignoreFilter2);

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

        [Test]
        public void MessageGrouping_SameSource_VerifyGrouping()
        {
            var subject = new MonitorWorkItem(new MonitorConfiguration())
                .AddMessageGroupingPatterns(ExportIntegrationErrorsStep.MessageGroupingPattern);

            using (TimeReseter.SetUtc(new DateTimeOffset(2015, 06, 13, 12, 00, 00, TimeSpan.Zero)))
            {
                subject.Add(Time.UtcNow.AddHours(-2), "Source", "Message. ErrorID: 1");
                subject.Add(Time.UtcNow.AddHours(-1), "Source", "Message. ErrorID: 2");
                subject.Add(Time.UtcNow, "Source", "Message. ErrorID: 3");

                MonitorEntry[] entries = subject.GetEntries(Target.Service);

                Assert.That(entries.Length, Is.EqualTo(1));
                Assert.That(entries[0].DateTime, Is.EqualTo(Time.UtcNow));
                Assert.That(entries[0].Source, Is.EqualTo("Source"));
                Assert.That(entries[0].Message, Is.EqualTo(@"Message. ErrorID: 3

--- Additional similar entries (Total: 3) ---

ErrorID: 2 (6/13/2015 11:00:00 AM +00:00)
ErrorID: 1 (6/13/2015 10:00:00 AM +00:00)"));
            }
        }

        [Test]
        public void MessageGrouping_DifferentSource_NoGrouping()
        {
            var subject = new MonitorWorkItem(new MonitorConfiguration())
                .AddMessageGroupingPatterns(ExportIntegrationErrorsStep.MessageGroupingPattern);

            var firstError = new MonitorEntry(Time.UtcNow.AddSeconds(-10), "SourceA", "Message. ErrorID: 1");
            var lastError = new MonitorEntry(Time.UtcNow, "SourceB", "Message. ErrorID: 2");

            subject.Add(firstError);
            subject.Add(lastError);

            MonitorEntry[] entries = subject.GetEntries(Target.Service);

            Assert.That(entries.Length, Is.EqualTo(2));
            Assert.That(entries[1], Is.SameAs(firstError));
            Assert.That(entries[0], Is.SameAs(lastError));
        }
	}
}