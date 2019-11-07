using System;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;

namespace Vertica.Integration.Tests.Infrastructure.Testing
{
    public static class ApplicationConfigurationTestingExtensions
    {
        public static ApplicationConfiguration ConfigureForUnitTest(this ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            return application
                .UseLiteServer(liteServer => liteServer
                    .HouseKeeping(houseKeeping => houseKeeping
                        .Interval(TimeSpan.FromMilliseconds(50))
                        .OutputStatusOnNumberOfIterations(1)))
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Logging(logging => logging
                    .TextWriter());
        }

        public static ApplicationConfiguration WithWaitBlock(this ApplicationConfiguration application, WaitBlock waitBlock, ConsoleWriterQueue queue = null)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            return application
                .WithConsoleWriterQueue(queue ?? new ConsoleWriterQueue())
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IWaitForShutdownRequest, WaitForWaitBlockToFinish>()
                        .Register(kernel => waitBlock)));
        }

        public static ApplicationConfiguration WithConsoleWriterQueue(this ApplicationConfiguration application, ConsoleWriterQueue queue)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            return application
                .Services(services => services
                    .Interceptors(interceptors => interceptors
                        .InterceptService<IConsoleWriter, ConsoleWriterQueue.RedirectInterceptor>())
                    .Advanced(advanced => advanced
                        .Register(kernel => queue)));
        }
    }
}