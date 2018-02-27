using System;
using System.Threading;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.Console.LiteServer.Heartbeat
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
                    .ShutdownTimeout(TimeSpan.FromSeconds(5))
                    .AddWorker<MyWorker>()
                    .HouseKeeping(houseKeeping => houseKeeping
                        .Interval(TimeSpan.FromSeconds(1))
                        .OutputStatusOnNumberOfIterations(10)
                        //.EnableHeartbeatLogging(heartbeatLogging => heartbeatLogging
                        //    .AddProvider<MyHeartbeatProvider>()
                        //    .Interval(TimeSpan.FromSeconds(5)))
                    )
                )))
            {
                context.Execute(nameof(LiteServerHost));
            }
        }
    }

    //public class MyHeartbeatProvider : IHeartbeatProvider
    //{
    //}

    public class MyWorker : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            if (context.InvocationCount == 4)
                return context.Exit();

            context.Console.WriteLine("Doing work...");

            return context.Wait(TimeSpan.FromSeconds(3));
        }
    }
}