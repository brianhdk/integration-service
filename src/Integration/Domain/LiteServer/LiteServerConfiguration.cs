using System;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Domain.LiteServer
{
	public class LiteServerConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly InternalConfiguration _configuration;

		private readonly ScanAddRemoveInstaller<IBackgroundServer> _servers;
		private readonly ScanAddRemoveInstaller<IBackgroundWorker> _workers; 

		internal LiteServerConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

		    _configuration = new InternalConfiguration();

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