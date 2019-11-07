using System;
using System.Collections.Generic;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Serilog;
using Vertica.Integration.Serilog.Infrastructure;
using Vertica.Integration.Serilog.Infrastructure.Castle.Windsor;
using Logger = Vertica.Integration.Serilog.Infrastructure.Logger;

namespace Vertica.Integration.Serilog
{
    public class SerilogConfiguration : IInitializable<ApplicationConfiguration>
    {
        private bool _skipSetGlobalStaticLogger;

        private IWindsorInstaller _defaultConnection;
        private readonly List<IWindsorInstaller> _connections;

        internal SerilogConfiguration(ApplicationConfiguration application)
	    {
		    if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application;

	        _connections = new List<IWindsorInstaller>();
	    }

		public ApplicationConfiguration Application { get; }

        public SerilogConfiguration DefaultLogger(Func<IKernel, ILogger> factory, bool setGlobalStaticLogger = true)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _defaultConnection = new SerilogLoggerInstaller(
                new DefaultLogger(factory, setGlobalStaticLogger));

            return this;
        }

        public SerilogConfiguration DefaultLogger(Logger logger, bool setGlobalStaticLogger = true)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _defaultConnection = new SerilogLoggerInstaller(
                new DefaultLogger(logger, setGlobalStaticLogger));
            
            return this;
        }

        public SerilogConfiguration AddLogger<TConnection>(TConnection connection)
            where TConnection : Logger
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _connections.Add(new SerilogLoggerInstaller<TConnection>(connection));

            return this;
        }

        [Obsolete("Don't use. Use the DefaultLogger method instead.")]
        public SerilogConfiguration Configure(Action<IKernel, LoggerConfiguration> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            return DefaultLogger(kernel =>
            {
                var configurationLocal = new LoggerConfiguration();
                configuration(kernel, configurationLocal);

                return configurationLocal.CreateLogger();
            }, !_skipSetGlobalStaticLogger);
        }

        /// <summary>
        /// This will not set the globally-shared <code>Log.Logger</code> property with the ILogger instance.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Don't use. Use the DefaultLogger method instead.")]
        public SerilogConfiguration SkipSetGlobalStaticLogger()
        {
            _skipSetGlobalStaticLogger = true;

            return this;
        }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            application.Services(services => services.Advanced(advanced =>
            {
                if (_defaultConnection != null)
                    advanced.Install(_defaultConnection);

                advanced.Install(_connections.ToArray());
            }));
        }
    }
}