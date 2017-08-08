using System;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Domain.LiteServer
{
    public class HouseKeepingConfiguration
    {
        private readonly InternalConfiguration _configuration;

        internal HouseKeepingConfiguration(LiteServerConfiguration liteServer, InternalConfiguration configuration)
        {
            if (liteServer == null) throw new ArgumentNullException(nameof(liteServer));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            LiteServer = liteServer;
            _configuration = configuration;
        }

        public LiteServerConfiguration LiteServer { get; }

        /// <summary>
        /// Overrides how frequently the HouseKeeping thread should monitor background servers and workers.
        /// Default is every 5th second.
        /// </summary>
        public HouseKeepingConfiguration Interval(TimeSpan interval)
        {
            _configuration.HouseKeepingInterval = interval;

            return this;
        }

        /// <summary>
        /// Overrides the number of iterations HouseKeeping should do before outputting the current status of background servers and workers.
        /// Default is on every 12th iteration (= every minute, if interval is 5 seconds).
        /// </summary>
        public HouseKeepingConfiguration OutputStatusOnNumberOfIterations(uint iteration)
        {
            _configuration.HouseKeepingOutputStatusOnNumberOfIterations = iteration;

            return this;
        }
    }
    public class LiteServerConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly InternalConfiguration _configuration;
	    private readonly HouseKeepingConfiguration _houseKeeping;

        private readonly ScanAddRemoveInstaller<IBackgroundServer> _servers;
		private readonly ScanAddRemoveInstaller<IBackgroundWorker> _workers; 

		internal LiteServerConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

		    _configuration = new InternalConfiguration();
		    _houseKeeping = new HouseKeepingConfiguration(this, _configuration);

		    _servers = new ScanAddRemoveInstaller<IBackgroundServer>(configure: x => x.LifeStyle.Is(LifestyleType.Transient));
		    _workers = new ScanAddRemoveInstaller<IBackgroundWorker>(configure: x => x.LifeStyle.Is(LifestyleType.Transient));

		    Application = application
		        .Hosts(hosts => hosts.Host<LiteServerHost>())
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Install(_servers)
                        .Install(_workers)));
		}

		public ApplicationConfiguration Application { get; }

		public LiteServerConfiguration OnStartup(Action<StartupActions> startup)
		{
			if (startup == null) throw new ArgumentNullException(nameof(startup));

			startup(_configuration.OnStartup);

			return this;
		}
		
		public LiteServerConfiguration OnShutdown(Action<ShutdownActions> shutdown)
		{
			if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));

			shutdown(_configuration.OnShutdown);

			return this;
		}

		/// <summary>
		/// Specifies the timeout (default 5 seconds) to wait for all tasks to complete their work when shutting down the server has been signaled.
		/// </summary>
		public LiteServerConfiguration ShutdownTimeout(TimeSpan timeout)
		{
			_configuration.ShutdownTimeout = timeout;

			return this;
		}

        /// <summary>
        /// Gives access to configure the internal "HouseKeeping" behaviour of LiteServer, e.g. monitoring background servers and workers.
        /// </summary>
	    public LiteServerConfiguration HouseKeeping(Action<HouseKeepingConfiguration> houseKeeping)
	    {
	        if (houseKeeping == null) throw new ArgumentNullException(nameof(houseKeeping));

	        houseKeeping(_houseKeeping);

	        return this;
	    }

        /// <summary>
        /// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IBackgroundServer"/> and/or <see cref="IBackgroundWorker"/>
        /// <para />
        /// </summary>
        /// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
        public LiteServerConfiguration AddFromAssemblyOfThis<T>()
		{
			_servers.AddFromAssemblyOfThis<T>();
			_workers.AddFromAssemblyOfThis<T>();

			return this;
		}

		/// <summary>
		/// Adds the specified <typeparamref name="TServer"/>.
		/// </summary>
		/// <typeparam name="TServer">Specifies the <see cref="IBackgroundServer"/> to be added.</typeparam>
		public LiteServerConfiguration AddServer<TServer>()
			where TServer : IBackgroundServer
		{
			_servers.Add<TServer>();

			return this;
		}

		/// <summary>
		/// Skips the specified <typeparamref name="TServer" />.
		/// </summary>
		/// <typeparam name="TServer">Specifies the <see cref="IBackgroundServer"/> that will be skipped.</typeparam>
		public LiteServerConfiguration RemoveServer<TServer>()
			where TServer : IBackgroundServer
		{
			_servers.Remove<TServer>();

			return this;
		}

		/// <summary>
		/// Adds the specified <typeparamref name="TWorker"/>.
		/// </summary>
		/// <typeparam name="TWorker">Specifies the <see cref="IBackgroundWorker"/> to be added.</typeparam>
		public LiteServerConfiguration AddWorker<TWorker>()
			where TWorker : IBackgroundWorker
		{
			_workers.Add<TWorker>();

			return this;
		}

		/// <summary>
		/// Skips the specified <typeparamref name="TWorker" />.
		/// </summary>
		/// <typeparam name="TWorker">Specifies the <see cref="IBackgroundWorker"/> that will be skipped.</typeparam>
		public LiteServerConfiguration RemoveWorker<TWorker>()
			where TWorker : IBackgroundWorker
		{
			_workers.Remove<TWorker>();

			return this;
		}

		/// <summary>
		/// Clears all registrations.
		/// </summary>
		public LiteServerConfiguration Clear()
		{
			_servers.Clear();
			_workers.Clear();

			return this;
		}

		void IInitializable<IWindsorContainer>.Initialized(IWindsorContainer container)
		{
			container.Register(
				Component.For<ILiteServerFactory>()
					.UsingFactoryMethod(kernel => new LiteServerFactory(kernel, _configuration)));
		}
		
		private class LiteServerFactory : ILiteServerFactory
		{
			private readonly IKernel _kernel;
			private readonly InternalConfiguration _configuration;

			public LiteServerFactory(IKernel kernel, InternalConfiguration configuration)
			{
				_kernel = kernel;
				_configuration = configuration;
			}

			public IDisposable Create()
			{
                return new LiteServerImpl(_kernel, _configuration);
			}
		}
	}
}