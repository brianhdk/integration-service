using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex;
using Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db;

namespace Vertica.Integration.Infrastructure
{
    public class AdvancedConfiguration : IInitializable<IWindsorContainer>
    {
	    private readonly IDictionary<Type, Tuple<Type, Type>> _types;
	    private readonly IDictionary<Type, Tuple<Func<object>, Func<object>>> _instances;

	    private readonly ShutdownConfiguration _shutdown;

	    internal AdvancedConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application;

	        _types = new Dictionary<Type, Tuple<Type, Type>>();
			_instances = new Dictionary<Type, Tuple<Func<object>, Func<object>>>();
			
			Register<ILogger, DbLogger, EventLogger>();
		    Register<IConfigurationRepository, DbConfigurationRepository, FileBasedConfigurationRepository>();
		    Register<IArchiveService, DbArchiveService, FileBasedArchiveService>();
	        Register<IDistributedMutex, DbDistributedMutex, ThrowingDistributedMutex>();
		    Register<IRuntimeSettings, AppConfigRuntimeSettings>();

		    Register(() => Environment.UserInteractive ? Console.Out : TextWriter.Null);

	        _shutdown = new ShutdownConfiguration(this);

            Application.Extensibility(extensibility => extensibility.Register(() => _shutdown));
        }

        public ApplicationConfiguration Application { get; }

		/// <summary>
		/// Registers a particular service with a particular implementation.
		/// </summary>
		/// <typeparam name="TService"></typeparam>
		/// <typeparam name="TImpl"></typeparam>
		/// <returns></returns>
		public AdvancedConfiguration Register<TService, TImpl>()
			where TService : class
			where TImpl : TService
		{
			return Register<TService, TImpl, TImpl>();
		}

		/// <summary>
		/// Registers a one or another service implementation depending on whether the IntegrationDb is disabled or not.
		/// </summary>
		/// <typeparam name="TService">The common service interface shared by the implementations.</typeparam>
		/// <typeparam name="TIntegrationDbEnabledImpl">The service implementation to use when we have an IntegrationDb available.</typeparam>
		/// <typeparam name="TIntegrationDbDisabledImpl">The (fallback) service implementation to use when IntegrationDb is disabled.</typeparam>
	    public AdvancedConfiguration Register<TService, TIntegrationDbEnabledImpl, TIntegrationDbDisabledImpl>()
			where TService : class
			where TIntegrationDbEnabledImpl : TService
			where TIntegrationDbDisabledImpl : TService
	    {
		    _types[typeof (TService)] = Tuple.Create(
				typeof (TIntegrationDbEnabledImpl), 
				typeof (TIntegrationDbDisabledImpl));

		    _instances[typeof(TService)] = null;

		    return this;
	    }

		public AdvancedConfiguration Register<TService>(Func<TService> instance)
			where TService : class
		{
			return Register(instance, instance);
		}

	    public AdvancedConfiguration Register<TService>(Func<TService> integrationDbEnabledInstance, Func<TService> integrationDbDisabledInstance)
			where TService : class
		{
		    if (integrationDbEnabledInstance == null) throw new ArgumentNullException(nameof(integrationDbEnabledInstance));
		    if (integrationDbDisabledInstance == null) throw new ArgumentNullException(nameof(integrationDbDisabledInstance));

			_instances[typeof(TService)] = Tuple.Create<Func<object>, Func<object>>(
				integrationDbEnabledInstance, 
				integrationDbDisabledInstance);

		    _types[typeof (TService)] = null;

			return this;
	    }

	    public AdvancedConfiguration Shutdown(Action<ShutdownConfiguration> shutdown)
	    {
		    if (shutdown == null) throw new ArgumentNullException(nameof(shutdown));

		    shutdown(_shutdown);

		    return this;
	    }

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
	    {
			bool disabled = false;
		    Application.Database(database => database
                .IntegrationDb(integrationDb => 
                    disabled = integrationDb.Disabled));

		    foreach (var pair in _types.Where(x => x.Value != null))
			    container.Register(Component.For(pair.Key).ImplementedBy(!disabled ? pair.Value.Item1 : pair.Value.Item2).LifestyleSingleton());

		    foreach (var pair in _instances.Where(x => x.Value != null))
			    container.Register(Component.For(pair.Key).Instance(!disabled ? pair.Value.Item1() : pair.Value.Item2()).LifestyleSingleton());
	    }
    }
}