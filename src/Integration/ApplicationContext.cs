using System;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Model.Exceptions;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration
{
	public sealed class ApplicationContext : IApplicationContext, IShutdown
    {
	    private readonly CancellationTokenSource _cancellation;
	    private readonly ApplicationConfiguration _configuration;

	    private readonly IWindsorContainer _container;
	    private readonly IArgumentsParser _parser;
	    private readonly IHost[] _hosts;
		private readonly TextWriter _writer;

		private readonly Lazy<Action> _disposed = new Lazy<Action>(() => () => { });

		internal ApplicationContext(Action<ApplicationConfiguration> application)
		{
            _cancellation = new CancellationTokenSource();
            _configuration = new ApplicationConfiguration();

			application?.Invoke(_configuration);

		    _configuration.RegisterDependency<IShutdown>(this);

		    _container = CastleWindsor.Initialize(_configuration);
            _parser = Resolve<IArgumentsParser>();
            _hosts = Resolve<IHostFactory>().GetAll();
		    _writer = Resolve<TextWriter>();

		    _writer.WriteLine("[Integration Service]: Started.");
        }

        public static IApplicationContext Create(Action<ApplicationConfiguration> application = null)
	    {
			return new ApplicationContext(application);
	    }

		public object Resolve(Type service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			return _container.Resolve(service);
		}

		public Array ResolveAll(Type service)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));

			return _container.ResolveAll(service);
		}

		public T Resolve<T>()
	    {
		    return _container.Resolve<T>();
	    }

	    public T[] ResolveAll<T>()
	    {
		    return _container.ResolveAll<T>();
	    }

	    public void Execute(params string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            Execute(_parser.Parse(args));
        }

        public void Execute(HostArguments args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            IHost[] hosts = _hosts.Where(x => x.CanHandle(args)).ToArray();

	        if (hosts.Length == 0)
		        throw new NoHostFoundException(args);

	        if (hosts.Length > 1)
		        throw new MultipleHostsFoundException(args, hosts);

	        try
	        {
				hosts[0].Handle(args);
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

            _writer.WriteLine("[Integration Service]: Shutdown requested.");

            _cancellation.Cancel();
        }

        public CancellationToken Token => _cancellation.Token;

        public void Dispose()
	    {
            if (_disposed.IsValueCreated)
			    throw new InvalidOperationException("ApplicationContext has already been disposed.");

            _disposed.Value();

            _writer.WriteLine("[Integration Service]: Shutting down.");

            if (!_cancellation.IsCancellationRequested)
            {
                try
                {
                    // if anything is running, we need to signal a cancellation
                    _cancellation.Cancel();
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }

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

            _writer.WriteLine("[Integration Service]: Shut down");

            _cancellation.Dispose();
			_container.Dispose();
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