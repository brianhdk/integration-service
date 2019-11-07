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
	    private Action<ConsoleOptions> _options;

	    internal HangfireConsoleConfiguration(HangfireConfiguration hangfire)
		{
		    if (hangfire == null) throw new ArgumentNullException(nameof(hangfire));

		    Hangfire = hangfire
                .Configuration((configuration, kernel) =>
		        {
		            if (_options != null)
		            {
		                var options = new ConsoleOptions();
		                _options(options);

		                configuration.UseConsole(options);
		            }
		            else
		            {
		                configuration.UseConsole();
		            }

		            configuration
		                .UseFilter(kernel.Resolve<SetPerformContextFilter>());
		        });
		}

		public HangfireConfiguration Hangfire { get; }

	    public HangfireConsoleConfiguration WithOptions(Action<ConsoleOptions> options)
	    {
	        if (options == null) throw new ArgumentNullException(nameof(options));

	        _options = options;

	        return this;
	    }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            application.Services(services => services
                .Advanced(advanced => advanced
                    .Register<SetPerformContextFilter>()
                    .Register<IHangfirePerformContextProvider, HangfirePerformContextProvider>()
                    .Install(Install.Service<HangfirePerThreadPerformContext>(x => x.LifestylePerThread())))
                .Interceptors(interceptors => interceptors
                    .InterceptService<IConsoleWriter, HangfireConsoleWriterInterceptor>()
                    .InterceptService<ILogger, HangfireLoggerInterceptor>()));
        }
    }
}