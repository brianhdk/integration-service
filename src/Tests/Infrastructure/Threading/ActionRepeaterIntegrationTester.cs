using System;
using System.Threading;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Threading;

namespace Vertica.Integration.Tests.Infrastructure.Threading
{
    [TestFixture(Category = "Slow,Integration")]
    [Ignore("Threading")]
    public class ActionRepeaterIntegrationTester
    {
        [Test]
        public void ActionRepeater_Tester()
        {
            var cancellation = new CancellationTokenSource();

            var delay = TimeSpan.FromMilliseconds(100);

            int iterations = 0;

            Action action = () =>
            {
                if (++iterations == 10)
                    cancellation.Cancel();

                Console.WriteLine($"{iterations} ");
            };

            using (ActionRepeater.Start(action, delay, cancellation.Token, Console.Out))
            {
                cancellation.Token.WaitHandle.WaitOne(2000);
            }
        }
    }
}