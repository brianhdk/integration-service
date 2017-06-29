using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .UseLiteServer(liteServer => liteServer
                    .AddFromAssemblyOfThis<SomeServer>())))
            {
                context.Execute(nameof(LiteServerHost));
            }
        }
    }

    public class SomeServer : IBackgroundServer, IRestartable
    {
        public Task Create(BackgroundServerContext context, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == 3)
                        throw new InvalidOperationException("I'm done!");

                    if (token.IsCancellationRequested)
                        return;

                    Thread.Sleep(1000);
                }
            }, token);
        }

        public bool ShouldRestart(RestartableContext context)
        {
            if (context.FailedCount < 3)
                return true;

            return false;
        }

        public override string ToString()
        {
            return nameof(SomeServer);
        }
    }

    public class SomeWorker : IBackgroundWorker, IRestartable
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            if (context.InvocationCount == 3)
                throw new InvalidOperationException("I'm done!");

            return context.Wait(TimeSpan.FromSeconds(1));
        }

        public bool ShouldRestart(RestartableContext context)
        {
            if (context.FailedCount < 3)
                return true;

            return false;
        }

        public override string ToString()
        {
            return nameof(SomeWorker);
        }
    }
}
