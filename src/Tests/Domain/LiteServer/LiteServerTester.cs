using System;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Tests.Domain.LiteServer
{
    [TestFixture]
    public class LiteServerTester
    {
        [Test]
        public void Nothing_To_Do()
        {
            using (var resetEvent = new ManualResetEvent(false))
            {
                var shutdown = new ShutdownHandler(resetEvent, TimeSpan.FromSeconds(2));
                var logger = Substitute.For<ILogger>();

                using (var context = ApplicationContext.Create(application => application
                    .UseLiteServer(liteServer => liteServer.OnStartup(startup => { }))
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)
                            .Register<IWaitForShutdownRequest>(kernel => shutdown)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }
            }
        }

        [Test]
        public void Worker_Failing_Ensure_Is_Logged()
        {
            using (var resetEvent = new ManualResetEvent(false))
            {
                var shutdown = new ShutdownHandler(resetEvent, TimeSpan.FromSeconds(2));
                var logger = Substitute.For<ILogger>();

                using (var context = ApplicationContext.Create(application => application
                    .UseLiteServer(liteServer => liteServer
                        .AddWorker<FailingWorker>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => logger)
                            .Register<IWaitForShutdownRequest>(kernel => shutdown)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }

                logger.Received(1).LogError(Arg.Is<InvalidOperationException>(x => x.Message == "failing"));
            }
        }

        public class FailingWorker : IBackgroundWorker
        {
            public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
            {
                throw new InvalidOperationException("failing");
            }
        }

        public class ShutdownHandler : IWaitForShutdownRequest
        {
            private readonly ManualResetEvent _resetEvent;
            private readonly TimeSpan _maxWaitTime;

            public ShutdownHandler(ManualResetEvent resetEvent, TimeSpan maxWaitTime)
            {
                _resetEvent = resetEvent;
                _maxWaitTime = maxWaitTime;
            }

            public void Wait()
            {
                _resetEvent.WaitOne(_maxWaitTime);
            }
        }

    }
}