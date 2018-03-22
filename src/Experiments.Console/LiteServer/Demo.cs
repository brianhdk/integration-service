using System;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db;
using Vertica.Utilities;

namespace Experiments.Console.LiteServer
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .UseLiteServer(liteServer => liteServer
                    .OnStartup(startup => startup
                        .Add(kernel => kernel.Resolve<IDeleteDbDistributedMutexLocksCommand>().Execute(Time.UtcNow)))
                    .AddWorker<MyWorker>()
                    .AddWorker<MyOtherWorker>()
                    .AddServer<MyServer>()
                    .AddServer<MyOtherServer>())))
            {
                context.Execute(nameof(LiteServerHost));
            }
        }
    }

    public class MyServer : IBackgroundServer
    {
        public Task Create(BackgroundServerContext context, CancellationToken token)
        {
            // Will fail from start - but the LiteServer will still run
            throw new NotImplementedException();
        }
    }

    public class MyOtherServer : IBackgroundServer
    {
        public Task Create(BackgroundServerContext context, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                int iterationCount = 0;

                while (!token.IsCancellationRequested)
                {
                    context.Console.WriteLine($"{nameof(MyOtherServer)}: Iteration#: {++iterationCount}");

                    token.WaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                }
            }, token);
        }
    }

    public class MyOtherWorker : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            context.Console.WriteLine($"{nameof(MyOtherWorker)}: Invocation#: {context.InvocationCount}");

            return context.Wait(TimeSpan.FromSeconds(2));
        }
    }

    public class MyWorker : IBackgroundWorker, IRestartable
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            if (context.InvocationCount < 5u)
                throw new InvalidOperationException();

            context.Console.WriteLine($"{nameof(MyOtherWorker)}: Invocation#: {context.InvocationCount}");
            
            return context.Wait(TimeSpan.FromSeconds(2));
        }

        public bool ShouldRestart(RestartableContext context)
        {
            return context.FailedCount < 5u;
        }
    }
}