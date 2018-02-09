using System;
using System.Linq;
using System.Threading;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Hosting;
using Vertica.Utilities;

namespace Vertica.Integration
{
	public sealed class ApplicationContext : IApplicationContext, IShutdown, IUptime
    {
        private readonly DateTimeOffset _startedAt;

        private readonly CancellationTokenSource _cancellation;
	    private readonly ApplicationConfiguration _configuration;

		private bool _isDisposed;

		internal ApplicationContext(Action<ApplicationConfiguration> application)
		{
            _startedAt = Time.UtcNow;

            _cancellation = new CancellationTokenSource();
            _configuration = new ApplicationConfiguration();

            // Executes all client-specific configuration
            application?.Invoke(_configuration);

            // Will instruct all extension-points that client-specific configurations have been completed.
            _configuration.Extensibility(extensibility =>
            {
                foreach (var subject in extensibility.OfType<IInitializable<ApplicationConfiguration>>())
                    subject.Initialized(_configuration);
            });

            // Creates the DI container
            Container = new WindsorContainer();
            Container.Kernel.AddFacility<TypedFactoryFacility>();
            Container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());
            
            // Allows all extension-points to initialize the DI container.
            _configuration.Extensibility(extensibility =>
            {
                foreach (var subject in extensibility.OfType<IInitializable<IWindsorContainer>>())
                    subject.Initialized(Container);
            });

            // Ensures that we "own" these specific service interfaces - which cannot be intercepted
            Container.Install(Install.Instance<IShutdown>(this, registration => registration.NamedAutomatically("Shutdown_23a911175572418588e212253a2dcf98")));
		    Container.Install(Install.Instance<IUptime>(this, registration => registration.NamedAutomatically("Uptime_d097752484954f1cb727633cdefc87a4")));

            // Allows all extensions-points to run code after the Integration Service is fully configured.
		    _configuration.Extensibility(extensibility =>
		    {
		        foreach (var subject in extensibility.OfType<IInitializable<IApplicationContext>>())
		            subject.Initialized(this);
		    });

            // Report that we're live and kicking
            WriteLine("[Integration Service]: Started at {0} (UTC).", _startedAt);
        }

        public static IApplicationContext Create(Action<ApplicationConfiguration> application = null)
	    {
			return new ApplicationContext(application);
	    }

		public object Resolve(Type service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

            EnsureNotDisposed();
            return Container.Resolve(service);
		}

		public Array ResolveAll(Type service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

            EnsureNotDisposed();
            return Container.ResolveAll(service);
		}

		public T Resolve<T>()
	    {
            EnsureNotDisposed();
            return Container.Resolve<T>();
	    }

	    public T[] ResolveAll<T>()
	    {
            EnsureNotDisposed();
            return Container.ResolveAll<T>();
	    }

	    public void Execute(params string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var parser = Resolve<IArgumentsParser>();

            Execute(parser.Parse(args));
        }

        public void Execute(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            IHost[] eligibleHosts = Resolve<IHostFactory>()
                .GetAll()
                .Where(x => x.CanHandle(args))
                .ToArray();

            if (eligibleHosts.Length == 0)
                throw new NoHostFoundException(args);

            if (eligibleHosts.Length > 1)
                throw new MultipleHostsFoundException(args, eligibleHosts);

            try
            {
                eligibleHosts[0].Handle(args);
            }
            catch (Exception ex)
            {
                LogException(ex);

                throw;
            }
        }

        public void WaitForShutdown()
        {
            Resolve<IWaitForShutdownRequest>().Wait();

            WriteLine("[Integration Service]: Shutdown requested.");

            EnsureCancelled(TimeSpan.FromMilliseconds(500));
        }

        public CancellationToken Token
        {
            get
            {
                EnsureNotDisposed();
                return _cancellation.Token;
            }
        }

        public void Dispose()
	    {
            lock (this)
	        {
                WriteLine("[Integration Service]: Shutting down.");

                EnsureCancelled();

                _configuration.Extensibility(extensibility =>
                {
                    foreach (var disposable in extensibility.OfType<IDisposable>())
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                });

                WriteLine("[Integration Service]: Shut down. Uptime: {0}", UptimeText);

                _cancellation.Dispose();
                Container.Dispose();

                _isDisposed = true;
            }
        }

        private IWindsorContainer Container { get; }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name, "ApplicationContext has already been disposed.");
        }

        public string UptimeText => Resolve<IUptimeTextGenerator>().GetUptimeText(_startedAt);

        private void WriteLine(string format, params object[] args)
        {
            Resolve<IConsoleWriter>().WriteLine(format, args);
        }

        private void EnsureCancelled(TimeSpan? cancelAfter = null)
	    {
	        if (!_cancellation.IsCancellationRequested)
	        {
                try
	            {
	                if (cancelAfter.HasValue)
	                {
                        // Allows for background threads to do clean-up, before we close down.
                        _cancellation.CancelAfter(cancelAfter.Value);
	                }
	                else
	                {
                        // Close down now.
	                    _cancellation.Cancel();
	                }
                }
	            catch (Exception ex)
	            {
                    LogException(ex);
	            }
	        }
	    }

	    private void LogException(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            if (exception is IIgnoreLogging)
                return;

            var logger = Resolve<ILogger>();

            try
            {
                logger.LogError(exception);
            }
            catch
            {
                if (logger is EventLogger)
                    throw;

	            var eventLogger = new EventLogger(
					Resolve<EventLoggerConfiguration>(), 
					Resolve<IRuntimeSettings>());

	            eventLogger.LogError(exception);
            }
        }
    }
}