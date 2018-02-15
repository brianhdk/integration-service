using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Castle.Windsor;
using Hangfire;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire.Console;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Hangfire
{
	public class HangfireConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly InternalConfiguration _configuration;
		private readonly ScanAddRemoveInstaller<IBackgroundProcess> _backgroundProcesses;
		
		internal HangfireConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.Hosts(hosts => hosts.Host<HangfireHost>());

			_configuration = new InternalConfiguration();
			_backgroundProcesses = new ScanAddRemoveInstaller<IBackgroundProcess>();
		}

		public ApplicationConfiguration Application { get; }

		public HangfireConfiguration Configuration(Action<IGlobalConfiguration> configuration)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return Configuration((innerConfiguration, kernel) => 
            {
                configuration(innerConfiguration);
            });
		}

	    public HangfireConfiguration Configuration(Action<IGlobalConfiguration, IKernel> configuration)
	    {
	        _configuration.Add(configuration);

	        return this;
	    }

	    public HangfireConfiguration EnableConsole(Action<HangfireConsoleConfiguration> console = null)
	    {
            Application.Extensibility(extensibility =>
            {
                var configuration = extensibility.Register(() => new HangfireConsoleConfiguration(this));

                console?.Invoke(configuration);
            });

            return this;
	    }

		public HangfireConfiguration WithServerOptions(Action<BackgroundJobServerOptions> serverOptions)
		{
			if (serverOptions == null) throw new ArgumentNullException(nameof(serverOptions));

			serverOptions(_configuration.ServerOptions);

			return this;
		}

		public HangfireConfiguration OnStartup(Action<StartupActions> startup)
		{
			if (startup == null) throw new ArgumentNullException(nameof(startup));

			startup(_configuration.OnStartup);

			return this;
		}
		
		public HangfireConfiguration OnShutdown(Action<ShutdownActions> shutdown)
		{
			if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));

			shutdown(_configuration.OnShutdown);

			return this;
		}

		/// <summary>
		/// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IBackgroundProcess"/>.
		/// <para />
		/// </summary>
		/// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
		public HangfireConfiguration AddFromAssemblyOfThis<T>()
		{
			_backgroundProcesses.AddFromAssemblyOfThis<T>();

			return this;
		}

		/// <summary>
		/// Adds Hangfire to <see cref="ILiteServerFactory"/> allowing Hangfire to run simultaneously with other servers.
		/// </summary>
		public HangfireConfiguration AddToLiteServer()
		{
			Application.UseLiteServer(server => server.AddServer<HangfireBackgroundServer>());

			return this;
		}

		/// <summary>
		/// Adds the specified <typeparamref name="TProcess"/>.
		/// </summary>
		/// <typeparam name="TProcess">Specifies the <see cref="IBackgroundProcess"/> to be added.</typeparam>
		public HangfireConfiguration Add<TProcess>()
			where TProcess : IBackgroundProcess
		{
			_backgroundProcesses.Add<TProcess>();

			return this;
		}

		/// <summary>
		/// Skips the specified <typeparamref name="TProcess" />.
		/// </summary>
		/// <typeparam name="TProcess">Specifies the <see cref="IBackgroundProcess"/> that will be skipped.</typeparam>
		public HangfireConfiguration Remove<TProcess>()
			where TProcess : IBackgroundProcess
		{
			_backgroundProcesses.Remove<TProcess>();

			return this;
		}

		/// <summary>
		/// Clears all registred BackgroundProcesses.
		/// </summary>
		public HangfireConfiguration Clear()
		{
			_backgroundProcesses.Clear();

			return this;
		}

		void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
		{
            container.Install(_backgroundProcesses);
		    container.Install(Install.Instance<IInternalConfiguration>(_configuration));
		    container.Install(Install.Service<IHangfireServerFactory, HangfireServerFactory>());

            // Assign a Storage as early as this, to support creating a brand-new Hangfire database part of the initial migration of Integration Service.
		    GlobalConfiguration.Configuration.UseStorage(new DelegatingJobStorage(container, _configuration));
		}

	    private class DelegatingJobStorage : JobStorage
	    {
	        private readonly IWindsorContainer _container;
	        private readonly InternalConfiguration _configuration;
	        private bool _initialized;

	        public DelegatingJobStorage(IWindsorContainer container, InternalConfiguration configuration)
	        {
	            _container = container;
	            _configuration = configuration;
	        }

	        public override IMonitoringApi GetMonitoringApi()
	        {
	            return Ensure(current => current.GetMonitoringApi());
	        }

	        public override IStorageConnection GetConnection()
	        {
	            return Ensure(current => current.GetConnection());
	        }

#pragma warning disable 618
	        public override IEnumerable<IServerComponent> GetComponents()
#pragma warning restore 618
	        {
	            return Ensure(current => current.GetComponents());
	        }

	        public override IEnumerable<IStateHandler> GetStateHandlers()
	        {
	            return Ensure(c => c.GetStateHandlers());
	        }

	        public override void WriteOptionsToLog(ILog logger)
	        {
	            Ensure(current =>
	            {
	                current.WriteOptionsToLog(logger);

                    // dummy value
	                return true;
	            });
	        }

	        public override string ToString()
	        {
	            return Ensure(current => current.ToString());
	        }

	        private T Ensure<T>(Func<JobStorage, T> withJobStorage)
	        {
	            if (!_initialized)
	            {
	                lock (this)
	                {
	                    if (!_initialized)
	                    {
	                        foreach (Action<IGlobalConfiguration, IKernel> configurer in _configuration)
	                            configurer(GlobalConfiguration.Configuration, _container.Kernel);

	                        _initialized = true;
	                    }
	                }
	            }

	            JobStorage current = Current;

	            if (current is DelegatingJobStorage)
	                throw new InvalidOperationException(@"You must apply a JobStorage part of bootstrapping Hangfire, see the following example which uses the SqlServerStorage (available from the ""Hangfire.SqlServer"" NuGet package):

.UseHangfire(hangfire => hangfire
    .Configuration(configuration => configuration
        .UseSqlServerStorage(""Integrated Security=SSPI;Data Source=.\\SQLExpress;Database=IntegrationService_Hangfire"", new SqlServerStorageOptions
        {
            // Defines how often to query the Hangfire database
            QueuePollInterval = TimeSpan.FromSeconds(5)
        })
    )
)");

	            return withJobStorage(Current);
	        }
        }
	}
}