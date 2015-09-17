using System;
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class LoggingConfiguration : IInitializable<IWindsorContainer>
    {
        private Type _logger;
        private TextWriter _console;

        internal LoggingConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application;
        }

        public ApplicationConfiguration Application { get; private set; }

        public LoggingConfiguration Use<T>() where T : Logger
        {
            _logger = typeof (T);

            return this;
        }

        public LoggingConfiguration NullLogger()
        {
            return Use<NullLogger>();
        }

        public LoggingConfiguration EventLogger()
        {
            return Use<EventLogger>();
        }

        public LoggingConfiguration Console(TextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");

            _console = writer;

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
	        if (_logger == null)
	        {
		        bool dbDisabled = true;
		        Application.Database(cfg => dbDisabled = cfg.IntegrationDbDisabled);

		        if (dbDisabled)
			        EventLogger();
		        else
			        Use<DefaultLogger>();
	        }

	        container.Register(Component.For<ILogger>().ImplementedBy(_logger));

            container.RegisterInstance(_console ?? (Environment.UserInteractive ? System.Console.Out : TextWriter.Null));
        }
    }
}