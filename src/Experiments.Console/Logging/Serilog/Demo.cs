using Castle.MicroKernel;
using Serilog;
using Vertica.Integration;
using Vertica.Integration.Serilog;
using Vertica.Integration.Serilog.Infrastructure;

namespace Experiments.Console.Logging.Serilog
{
    public static class Demo
    {
        public static void Run()
        {
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .UseSerilog(serilog => serilog
                    .DefaultLogger(CreateLogger)
                    // Alternative to creating the default logger
                    //.DefaultLogger(new DefaultLogger())
                    .AddLogger(new AnotherLogger()))))
            {
                ILogger defaultLogger = context.Resolve<ILogger>();
                ILogger defaultLogger2 = context.Resolve<ILoggerFactory>().Logger;
                ILogger anotherLogger = context.Resolve<ILoggerFactory<AnotherLogger>>().Logger;
            }
        }

        private static ILogger CreateLogger(IKernel arg)
        {
            return new LoggerConfiguration().CreateLogger();
        }
    }

    public class DefaultLogger : Logger
    {
        protected override ILogger Create(IKernel kernel)
        {
            return new LoggerConfiguration().CreateLogger();
        }
    }

    public class AnotherLogger : Logger
    {
        protected override ILogger Create(IKernel kernel)
        {
            return new LoggerConfiguration().CreateLogger();
        }
    }
}