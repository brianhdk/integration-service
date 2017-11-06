using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;

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
                    .AddWorker<MyWorker>()
                    .AddWorker<MyOtherWorker>()
                    .AddServer<MyServer>()
                    .AddServer<MyOtherServer>())))
            {
                context.Execute(nameof(LiteServerHost));
            }
        }
    }

    public class MyOtherServer : IBackgroundServer
    {
        public Task Create(BackgroundServerContext context, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }

    public class MyServer : IBackgroundServer
    {
        public Task Create(BackgroundServerContext context, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }

    public class MyOtherWorker : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }

    public class MyWorker : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}