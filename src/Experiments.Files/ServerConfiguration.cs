using System;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Experiments.Files
{
	public class ServerConfiguration : IInitializable<IWindsorContainer>
	{
		private readonly InternalConfiguration _configuration;

		private readonly ScanAddRemoveInstaller<IBackgroundOperation> _operations;
		private readonly ScanAddRemoveInstaller<IBackgroundRepeatable> _repeatables; 

		internal ServerConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application
				.Hosts(hosts => hosts.Host<ServerHost>());

			_configuration = new InternalConfiguration();

			_operations = new ScanAddRemoveInstaller<IBackgroundOperation>(x => x.LifeStyle.Is(LifestyleType.Transient));
			_repeatables = new ScanAddRemoveInstaller<IBackgroundRepeatable>(x => x.LifeStyle.Is(LifestyleType.Transient));
		}

		public ApplicationConfiguration Application { get; }

		public ServerConfiguration OnStartup(Action<StartupActions> startup)
		{
			if (startup == null) throw new ArgumentNullException(nameof(startup));

			startup(_configuration.OnStartup);

			return this;
		}
		
		public ServerConfiguration OnShutdown(Action<ShutdownActions> shutdown)
		{
			if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));

			shutdown(_configuration.OnShutdown);

			return this;
		}

		/// <summary>
		/// Specifies the timeout (default 5 seconds) to wait for all tasks to complete their work when shutting down the server has been signaled.
		/// </summary>
		public ServerConfiguration WaitOnTasksTimeout(TimeSpan timeout)
		{
			_configuration.WaitOnTasksTimeout = timeout;

			return this;
		}

		/// <summary>
		/// Scans the assembly of the defined <typeparamref name="T"></typeparamref> for public classes inheriting <see cref="IBackgroundOperation"/> and/or <see cref="IBackgroundRepeatable"/>
		/// <para />
		/// </summary>
		/// <typeparam name="T">The type in which its assembly will be scanned.</typeparam>
		public ServerConfiguration AddFromAssemblyOfThis<T>()
		{
			_operations.AddFromAssemblyOfThis<T>();
			_repeatables.AddFromAssemblyOfThis<T>();

			return this;
		}

		/// <summary>
		/// Adds the specified <typeparamref name="TOperation"/>.
		/// </summary>
		/// <typeparam name="TOperation">Specifies the <see cref="IBackgroundOperation"/> to be added.</typeparam>
		public ServerConfiguration AddOperation<TOperation>()
			where TOperation : IBackgroundOperation
		{
			_operations.Add<TOperation>();

			return this;
		}

		/// <summary>
		/// Skips the specified <typeparamref name="TBackgroundOperation" />.
		/// </summary>
		/// <typeparam name="TBackgroundOperation">Specifies the <see cref="IBackgroundOperation"/> that will be skipped.</typeparam>
		public ServerConfiguration RemoveOperation<TBackgroundOperation>()
			where TBackgroundOperation : IBackgroundOperation
		{
			_operations.Remove<TBackgroundOperation>();

			return this;
		}

		/// <summary>
		/// Adds the specified <typeparamref name="TRepeatedWork"/>.
		/// </summary>
		/// <typeparam name="TRepeatedWork">Specifies the <see cref="IBackgroundRepeatable"/> to be added.</typeparam>
		public ServerConfiguration AddRepeatable<TRepeatedWork>()
			where TRepeatedWork : IBackgroundRepeatable
		{
			_repeatables.Add<TRepeatedWork>();

			return this;
		}

		/// <summary>
		/// Skips the specified <typeparamref name="TRepeatedWork" />.
		/// </summary>
		/// <typeparam name="TRepeatedWork">Specifies the <see cref="IBackgroundRepeatable"/> that will be skipped.</typeparam>
		public ServerConfiguration RemoveRepeatable<TRepeatedWork>()
			where TRepeatedWork : IBackgroundRepeatable
		{
			_repeatables.Remove<TRepeatedWork>();

			return this;
		}

		/// <summary>
		/// Clears all registrations.
		/// </summary>
		public ServerConfiguration Clear()
		{
			_operations.Clear();
			_repeatables.Clear();

			return this;
		}

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
		{
			container.Install(_operations);
			container.Install(_repeatables);

			container.Register(
				Component.For<IServerFactory>()
					.UsingFactoryMethod(kernel => new ServerFactory(kernel, _configuration)));
		}
		
		private class ServerFactory : IServerFactory
		{
			private readonly IKernel _kernel;
			private readonly InternalConfiguration _configuration;

			public ServerFactory(IKernel kernel, InternalConfiguration configuration)
			{
				_kernel = kernel;
				_configuration = configuration;
			}

			public IDisposable Create()
			{
				return new Server(_kernel, _configuration);
			}
		}
	}
}