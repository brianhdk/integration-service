using System;
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
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

            _logger = typeof (DefaultLogger);
        }

        public ApplicationConfiguration Application { get; private set; }

        public LoggingConfiguration Use<T>() where T : Logger
        {
            _logger = typeof (T);

            return this;
        }

        public LoggingConfiguration Console(TextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");

            _console = writer;

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.Register(
                Component.For<ILogger>()
                    .ImplementedBy(_logger));

            // TODO: Find a way for the Console to know whether it should be Console.Out or TextWriter.Null
            //  - windows task scheduler should be null (perhaps?)
            //  - windows service should be null => RunTaskFromWindowsService
            //  - webapi should be null => StartWebApiHost

            container.Register(
                Component.For<TextWriter>()
                    .UsingFactoryMethod(() => _console ?? System.Console.Out));
        }
    }
}