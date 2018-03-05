using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.LiteServer.Heartbeat;
using Vertica.Integration.Portal;
using Vertica.Integration.WebApi;

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
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            // Specify which URL WebAPI should listen on.
                            .Set("WebApi.Url", "http://localhost:8154")
                            .Set("ConfigurationServiceBasedHeartbeatLoggingRepository.MaximumNumberOfEntries", "3")
                            .Set("ConfigurationServiceBasedHeartbeatLoggingRepository.MaximumAge", "00:00:15"))))
                .UseLiteServer(liteServer => liteServer
                    .AddWorker<MyWorker>()
                    .Heartbeat(heartbeat => heartbeat
                        .EnableLogging(logging => logging
                            .Interval(TimeSpan.FromSeconds(5)))
                        .AddProvider<MySystemUptimeHeartbeatProvider>())
                    .HouseKeeping(houseKeeping => houseKeeping
                        .Interval(TimeSpan.FromSeconds(1))
                        .OutputStatusOnNumberOfIterations(10)))
                .UseWebApi(webApi => webApi
                    .AddToLiteServer()
                    .WithPortal())))
            {
                context.Execute(nameof(LiteServerHost));
            }
        }
    }

    public class MySystemUptimeHeartbeatProvider : IHeartbeatProvider, IDisposable
    {
        private readonly PerformanceCounter _uptime;

        public MySystemUptimeHeartbeatProvider()
        {
            _uptime = new PerformanceCounter("System", "System Up Time");
            _uptime.NextValue();
        }

        public IEnumerable<string> CollectHeartbeatMessages(CancellationToken token)
        {
            yield return $"System Uptime: {TimeSpan.FromSeconds(_uptime.NextValue())}";
        }

        public void Dispose()
        {
            _uptime.Dispose();
        }
    }

    public class MyWorker : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            if (context.InvocationCount == 10)
                return context.Exit();

            return context.Wait(TimeSpan.FromSeconds(1));
        }
    }
}