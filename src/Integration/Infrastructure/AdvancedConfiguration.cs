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

namespace Vertica.Integration.Infrastructure
{
    public class AdvancedConfiguration : IInitializable<IWindsorContainer>
    {
	    private readonly IDictionary<Type, Tuple<Type, Type>> _types;
	    private readonly IDictionary<Type, Tuple<Func<object>, Func<object>>> _instances;

	    internal AdvancedConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application;

	        _types = new Dictionary<Type, Tuple<Type, Type>>();
			_instances = new Dictionary<Type, Tuple<Func<object>, Func<object>>>();

		    Register<ILogger, DbLogger, EventLogger>();
		    Register<IConfigurationRepository, DbConfigurationRepository, FileBasedConfigurationRepository>();
		    Register<IArchiveService, DbArchiveService, FileBasedArchiveService>();
		    Register<IRuntimeSettings, AppConfigRuntimeSettings>();

		    Register(() => Environment.UserInteractive ? Console.Out : TextWriter.Null);
        }

        public ApplicationConfiguration Application { get; private set; }

		public AdvancedConfiguration Register<TService, TImpl>()
			where TService : class
			where TImpl : TService
		{
			return Register<TService, TImpl, TImpl>();
		}

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
		    if (integrationDbEnabledInstance == null) throw new ArgumentNullException("integrationDbEnabledInstance");
		    if (integrationDbDisabledInstance == null) throw new ArgumentNullException("integrationDbDisabledInstance");

			_instances[typeof(TService)] = Tuple.Create<Func<object>, Func<object>>(
				integrationDbEnabledInstance, 
				integrationDbDisabledInstance);

		    _types[typeof (TService)] = null;

			return this;
	    }

	    void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
	    {
			bool disabled = false;
		    Application.Database(database => disabled = database.IntegrationDbDisabled);

		    foreach (var pair in _types.Where(x => x.Value != null))
			    container.Register(Component.For(pair.Key).ImplementedBy(!disabled ? pair.Value.Item1 : pair.Value.Item2));

		    foreach (var pair in _instances.Where(x => x.Value != null))
			    container.Register(Component.For(pair.Key).Instance(!disabled ? pair.Value.Item1() : pair.Value.Item2()));
	    }
    }
}