using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.DynamicProxy;
using NUnit.Framework;
using Vertica.Integration.ConsoleHost;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;
using Vertica.Integration.Tests.Infrastructure;

namespace Vertica.Integration.Tests.ConsoleHost
{
    [TestFixture]
    public class WindowsServiceInterceptorTester
    {
        [Test]
        public void ExecuteHost_NotInWindowsServiceContext_AssertExecutionOrder()
        {
            var consoleWriterQueue = new ConsoleWriterQueue();

            using (var testContext = new TestContext(shouldHandleAsWindowsService: false))
            {
                Execute(testContext, consoleWriterQueue);
            }

            Console.Write(consoleWriterQueue.ToString());

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

            using (var testContext = new TestContext(shouldHandleAsWindowsService: true))
            {
                Execute(testContext, consoleWriterQueue);
            }

            Console.Write(consoleWriterQueue.ToString());

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

        private static void Execute(TestContext testContext, ConsoleWriterQueue consoleWriterQueue)
        {
            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .UseConsoleHost()
                .Hosts(hosts => hosts
                    .Clear()
                    .Host<MyHost>())
                .Services(services => services
                    .Interceptors(interceptors => interceptors
                        .InterceptService<IConsoleWriter, RedirectToQueueInterceptor>())
                    .Advanced(advanced => advanced
                        .Register(kernel => testContext)
                        .Register(kernel => consoleWriterQueue)
                        .Register<IWindowsServiceHandler, FakeWindowsServiceHandler>()
                        .Register<IWaitForShutdownRequest, ShutdownHandler>()
                    )
                )
            ))
            {
                context.Execute(nameof(MyHost));
            }
        }

        public class ShutdownHandler : IWaitForShutdownRequest
        {
            private readonly ConsoleWriterQueue _consoleWriterQueue;
            private readonly TestContext _testContext;

            public ShutdownHandler(ConsoleWriterQueue consoleWriterQueue, TestContext testContext)
            {
                _consoleWriterQueue = consoleWriterQueue;
                _testContext = testContext;
            }

            public void Wait()
            {
                _consoleWriterQueue.Enqueue("Shutdown.Waiting");

                _testContext.Wait();

                _consoleWriterQueue.Enqueue("Shutdown.Waited");
            }
        }

        public class RedirectToQueueInterceptor : IInterceptor
        {
            private readonly ConsoleWriterQueue _consoleWriterQueue;

            public RedirectToQueueInterceptor(ConsoleWriterQueue consoleWriterQueue)
            {
                _consoleWriterQueue = consoleWriterQueue;
            }

            public void Intercept(IInvocation invocation)
            {
                var message = (string)invocation.Arguments.FirstOrDefault();

                if (message != null)
                {
                    var args = (object[])invocation.Arguments.ElementAtOrDefault(1);

                    if (args != null)
                        message = string.Format(message, args);

                    _consoleWriterQueue.Enqueue(message);
                }
            }
        }

        public class TestContext : IDisposable
        {
            public TestContext(bool shouldHandleAsWindowsService)
            {
                ShouldHandleAsWindowsService = shouldHandleAsWindowsService;

                ResetEvent = new ManualResetEvent(false);
            }

            public bool ShouldHandleAsWindowsService { get; }
            public ManualResetEvent ResetEvent { get; }

            public void Wait()
            {
                ResetEvent.WaitOne(TimeSpan.FromSeconds(5));
            }

            public void Dispose()
            {
                ResetEvent.Dispose();
            }
        }

        public class FakeWindowsServiceHandler : IWindowsServiceHandler
        {
            private readonly TestContext _testContext;
            private readonly ConsoleWriterQueue _consoleWriterQueue;

            public FakeWindowsServiceHandler(ConsoleWriterQueue consoleWriterQueue, TestContext testContext)
            {
                _consoleWriterQueue = consoleWriterQueue;
                _testContext = testContext;
            }

            public bool Handle(HostArguments args, HandleAsWindowsService service)
            {
                if (_testContext.ShouldHandleAsWindowsService)
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

        public class ConsoleWriterQueue
        {
            private readonly Queue<string> _messages;
            private readonly List<string> _history;

            public ConsoleWriterQueue()
            {
                _messages = new Queue<string>();
                _history = new List<string>();
            }

            public void Enqueue(string message)
            {
                _messages.Enqueue(message);
                _history.Add(message);
            }

            public string Dequeue()
            {
                return _messages.Dequeue();
            }

            public int Count => _messages.Count;

            public override string ToString()
            {
                return string.Join(Environment.NewLine, _history);
            }
        }

        public class MyHost : IHost
        {
            private readonly IShutdown _shutdown;
            private readonly TestContext _testContext;
            private readonly ConsoleWriterQueue _consoleWriterQueue;

            public MyHost(IShutdown shutdown, ConsoleWriterQueue consoleWriterQueue, TestContext testContext)
            {
                _shutdown = shutdown;
                _consoleWriterQueue = consoleWriterQueue;
                _testContext = testContext;
            }

            public bool CanHandle(HostArguments args)
            {
                return true;
            }

            public void Handle(HostArguments args)
            {
                _consoleWriterQueue.Enqueue("Host.Handling");

                if (!_testContext.ShouldHandleAsWindowsService)
                {
                    _consoleWriterQueue.Enqueue("Host.ManuallyRelease");
                    _testContext.ResetEvent.Set();
                }

                _shutdown.WaitForShutdown();

                _consoleWriterQueue.Enqueue("Host.Handled");
            }

            public string Description => "Some description";
        }
    }
}