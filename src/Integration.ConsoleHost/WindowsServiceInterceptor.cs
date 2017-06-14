using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;
using Vertica.Utilities.Patterns;

namespace Vertica.Integration.ConsoleHost
{
    internal class WindowsServiceInterceptor : IInterceptor
    {
        private readonly IWindowsServiceHandler _windowsServiceHandler;
        private readonly IShutdown _shutdown;
        private readonly ILogger _logger;

        private readonly IsHostHandleSpecification _isHostHandleSpec;
        private readonly IsWaitForShutdownRequestSpecification _isWaitForShutdownRequestSpec;

        private WindowsServiceWaitHandle _waitHandle;

        public WindowsServiceInterceptor(IWindowsServiceHandler windowsServiceHandler, IShutdown shutdown, ILogger logger)
        {
            _windowsServiceHandler = windowsServiceHandler;
            _shutdown = shutdown;
            _logger = logger;

            _isHostHandleSpec = new IsHostHandleSpecification();
            _isWaitForShutdownRequestSpec = new IsWaitForShutdownRequestSpecification();
        }

        public void Intercept(IInvocation invocation)
        {
            if (_isHostHandleSpec.IsSatisfiedBy(invocation))
            {
                var host = invocation.InvocationTarget as IHost;
                var arguments = invocation.Arguments.FirstOrDefault() as HostArguments;

                if (host != null && arguments != null)
                {
                    var handle = new HandleAsWindowsService(host.Name(), host.Name(), host.Description, () =>
                    {
                        // Instantiate a WaitHandle to signal that we're running as a Windows Service
                        return _waitHandle = new WindowsServiceWaitHandle(invocation.Proceed, _shutdown.Token, _logger);
                    });

                    // If we're not a windows service, then just go ahead and execute the Host-implementation.
                    if (!_windowsServiceHandler.Handle(arguments, handle))
                        invocation.Proceed();
                }
            }
            else if (_isWaitForShutdownRequestSpec.IsSatisfiedBy(invocation))
            {
                if (_waitHandle != null)
                {
                    // Wait for Windows Service to signal a shutdown/stop
                    _waitHandle.Wait();
                }
                else
                {
                    // Execute the underlying Wait()-method
                    invocation.Proceed();
                }
            }
            else
            {
                // Execute the underlying method
                invocation.Proceed();
            }
        }

        private class IsHostHandleSpecification : Specification<IInvocation>
        {
            public override bool IsSatisfiedBy(IInvocation item)
            {
                return
                    item.InvocationTarget is IHost &&
                    item.Method.Name == nameof(IHost.Handle);
            }
        }

        private class IsWaitForShutdownRequestSpecification : Specification<IInvocation>
        {
            public override bool IsSatisfiedBy(IInvocation item)
            {
                return
                    item.InvocationTarget is IWaitForShutdownRequest &&
                    item.Method.Name == nameof(IWaitForShutdownRequest.Wait);
            }
        }

        private class WindowsServiceWaitHandle : IDisposable
        {
            private readonly ManualResetEvent _waitHandle;
            private readonly ILogger _logger;
            private readonly Task _task;

            public WindowsServiceWaitHandle(Action action, CancellationToken token, ILogger logger)
            {
                _waitHandle = new ManualResetEvent(false);
                _logger = logger;

                // Important: This should be the last thing to do
                _task = Task.Factory.StartNew(action, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            }

            public void Wait()
            {
                // Blocks until the Windows Service has been signaled to stop/shutdown.
                _waitHandle.WaitOne();
            }

            public void Dispose()
            {
                // The Windows Service signaled a stop/shutdown, so we'll signal our WaitHandle.
                _waitHandle.Set();

                if (!_task.IsCompleted)
                {
                    try
                    {
                        // Allow the underlying task 30 seconds to finish after receiving the signal.
                        _task.Wait(TimeSpan.FromSeconds(30));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex);
                    }
                }

                try
                {
                    if (_task.IsCompleted)
                        _task.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex);
                }

                _waitHandle.Dispose();
            }
        }
    }
}