using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Tests.Domain.Monitoring
{
    [TestFixture(Category = "Integration,Slow", Ignore = true)]
    public class PingUrlsStepIntegrationTester
    {
        [Test]
        public void Execute()
        {
            var configurationService = Substitute.For<IConfigurationService>();
            var configuration = new PingUrlsConfiguration
            {
                Urls = new[]
                {
                    "http://www.google.com"
                },
                MaximumWaitTimeSeconds = 10
            };

            configurationService.Get<PingUrlsConfiguration>().Returns(configuration);

            var subject = new PingUrlsStep(configurationService, new HttpClientFactory());

            var workItem = new MonitorWorkItem(Time.UtcNow);

            subject.Execute(workItem, Substitute.For<ILog>());

            string messages = String.Join(Environment.NewLine,
                workItem.GetEntries(Target.Service).Select(x => x.Message));

            Assert.IsEmpty(messages);
        }
    }
}