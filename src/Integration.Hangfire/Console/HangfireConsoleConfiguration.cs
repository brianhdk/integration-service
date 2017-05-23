using System;
using Hangfire;
using Hangfire.Console;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Hangfire.Console
{
	public class HangfireConsoleConfiguration : IInitializable<ApplicationConfiguration>
	{
		internal HangfireConsoleConfiguration(HangfireConfiguration hangfire)
		{
		    if (hangfire == null) throw new ArgumentNullException(nameof(hangfire));

		    Hangfire = hangfire
                .Configuration((configuration, kernel) => configuration
                        .UseConsole(new ConsoleOptions())
                        .UseFilter(kernel.Resolve<SetPerformContextFilter>()));
		}

		public HangfireConfiguration Hangfire { get; }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            application.Services(services => services
                .Advanced(advanced => advanced
                    .Register<SetPerformContextFilter>()
                    .Register<HangfirePerformContextFactory>()
                    .Install(Install.Service<HangfirePerThreadPerformContext>(x => x.LifestylePerThread())))
                .Interceptors(interceptors => interceptors
                    .InterceptService<IConsoleWriter, HangfireConsoleWriterInterceptor>()
                    .InterceptService<ILogger, HangfireLoggerInterceptor>()));
        }
    }
}