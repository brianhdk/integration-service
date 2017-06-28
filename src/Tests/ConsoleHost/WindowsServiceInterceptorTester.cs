using System;
using System.Threading;
using NUnit.Framework;
using Vertica.Integration.ConsoleHost;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;
using Vertica.Integration.Tests.Infrastructure.Testing;

namespace Vertica.Integration.Tests.ConsoleHost
{
    [TestFixture]
    public class WindowsServiceInterceptorTester
    {
        [Test]
        public void ExecuteHost_NotInWindowsServiceContext_AssertExecutionOrder()
        {
            var consoleWriterQueue = new ConsoleWriterQueue();

            using (var waitBlock = new WaitBlock())
            {
                Execute(waitBlock, consoleWriterQueue);
            }

            StringAssert.Contains("[Integration Service]: Started", consoleWriterQueue.Dequeue());
            StringAssert.Contains("NotWindowsService", consoleWriterQueue.Dequeue());
            StringAssert.Contains("Host.Handling", consoleWriterQueue.Dequeue());
            StringAssert.Contains("Host.ManuallyRelease", consoleWriterQueue.Dequeue());
            StringAssert.Contains("Shutdown.Waiting", consoleWriterQueue.Dequeue());
            StringAssert.Contains("Shutdown.Waited", consoleWriterQueue.Dequeue());
            StringAssert.Contains("[Integration Service]: Shutdown requested.", consoleWriterQueue.Dequeue());
            StringAssert.Contains("Host.Handled", consoleWriterQueue.Dequeue());
            StringAssert.Contains("[Integration Service]: Shutting down", consoleWriterQueue.Dequeue());
            StringAssert.Contains("[Integration Service]: Shut down.", consoleWriterQueue.Dequeue());

            Assert.That(consoleWriterQueue.Count, Is.Zero);
        }

        [Test]
        public void ExecuteHost_InWindowsServiceContext_AssertExecutionOrder()
        {
            var consoleWriterQueue = new ConsoleWriterQueue();

            using (var waitBlock = new WaitBlock())
            {
                Execute(waitBlock, consoleWriterQueue, asWindowsService: true);
            }

            StringAssert.Contains("[Integration Service]: Started", consoleWriterQueue.Dequeue());
            StringAssert.Contains("WindowsService.Starting", consoleWriterQueue.Dequeue());
            StringAssert.Contains("WindowsService.Running", consoleWriterQueue.Dequeue());
            StringAssert.Contains("Host.Handling", consoleWriterQueue.Dequeue());
            StringAssert.Contains("WindowsService.Stopping", consoleWriterQueue.Dequeue());
            StringAssert.Contains("[Integration Service]: Shutdown requested.", consoleWriterQueue.Dequeue());
            StringAssert.Contains("Host.Handled", consoleWriterQueue.Dequeue());
            StringAssert.Contains("WindowsService.Stopped", consoleWriterQueue.Dequeue());
            StringAssert.Contains("[Integration Service]: Shutting down", consoleWriterQueue.Dequeue());
            StringAssert.Contains("[Integration Service]: Shut down.", consoleWriterQueue.Dequeue());

            Assert.That(consoleWriterQueue.Count, Is.Zero);
        }

        private static void Execute(WaitBlock waitBlock, ConsoleWriterQueue consoleWriterQueue, bool asWindowsService = false)
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .WithWaitBlock(waitBlock, consoleWriterQueue)
                .UseConsoleHost()
                .Hosts(hosts => hosts
                    .Clear()
                    .Host<MyHost>())
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("ShouldHandleAsWindowsService", asWindowsService ? Boolean.TrueString : Boolean.FalseString))
                        .Register<IWindowsServiceHandler, FakeWindowsServiceHandler>()
                    )
                )
            ))
            {
                context.Execute(nameof(MyHost));
            }
        }

        public class FakeWindowsServiceHandler : IWindowsServiceHandler
        {
            private readonly ConsoleWriterQueue _consoleWriterQueue;
            private readonly bool _shouldHandleAsWindowsService;

            public FakeWindowsServiceHandler(ConsoleWriterQueue consoleWriterQueue, IRuntimeSettings runtimeSettings)
            {
                _consoleWriterQueue = consoleWriterQueue;
                _shouldHandleAsWindowsService = string.Equals(Boolean.TrueString, runtimeSettings["ShouldHandleAsWindowsService"]);
            }

            public bool Handle(HostArguments args, HandleAsWindowsService service)
            {
                if (_shouldHandleAsWindowsService)
                {
                    _consoleWriterQueue.Enqueue("WindowsService.Starting");

                    using (service.OnStartFactory())
                    {
                        // simulates that windows service is started
                        _consoleWriterQueue.Enqueue("WindowsService.Running");

                        // Block - simulates that windows service is running
                        Thread.Sleep(10);

                        // Simulates that Windows Service is shutting down
                        _consoleWriterQueue.Enqueue("WindowsService.Stopping");
                    }

                    _consoleWriterQueue.Enqueue("WindowsService.Stopped");

                    return true;
                }

                _consoleWriterQueue.Enqueue("NotWindowsService");
                return false;
            }
        }

        public class MyHost : IHost
        {
            private readonly IShutdown _shutdown;
            private readonly ConsoleWriterQueue _consoleWriterQueue;
            private readonly WaitBlock _waitBlock;
            private readonly bool _shouldHandleAsWindowsService;

            public MyHost(IShutdown shutdown, ConsoleWriterQueue consoleWriterQueue, WaitBlock waitBlock, IRuntimeSettings runtimeSettings)
            {
                _shutdown = shutdown;
                _consoleWriterQueue = consoleWriterQueue;
                _waitBlock = waitBlock;
                _shouldHandleAsWindowsService = string.Equals(Boolean.TrueString, runtimeSettings["ShouldHandleAsWindowsService"]);
            }

            public bool CanHandle(HostArguments args)
            {
                return true;
            }

            public void Handle(HostArguments args)
            {
                _consoleWriterQueue.Enqueue("Host.Handling");

                if (!_shouldHandleAsWindowsService)
                {
                    _consoleWriterQueue.Enqueue("Host.ManuallyRelease");
                    _waitBlock.Release();
                }

                _shutdown.WaitForShutdown();

                _consoleWriterQueue.Enqueue("Host.Handled");
            }

            public string Description => "Some description";
        }
    }
}