using System;
using System.Linq;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Remote;
using Vertica.Integration.Model;

namespace Vertica.Integration.Tests.Domain.Monitoring
{
    [TestFixture(Category = "Integration,Slow", Ignore = "Requires Internet connection")]
    public class PingUrlsStepIntegrationTester
    {
        [Test]
        public void Execute()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            var configuration = new MonitorConfiguration
            {
                PingUrls =
                {
                    Urls = new[]
                    {
                        "http://www.google.com"
                    },
                    MaximumWaitTimeSeconds = 10
                }
            };

            var subject = new PingUrlsStep(new HttpClientFactory());

            var workItem = new MonitorWorkItem(configuration);

            subject.Execute(workItem, Substitute.For<ITaskExecutionContext>());

            string messages = string.Join(Environment.NewLine,
                workItem.GetEntries(Target.Service).Select(x => x.Message));

            Assert.IsEmpty(messages);
        }
    }
}