using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public static class TextFileLoggerExtensions
    {
        public static LoggingConfiguration UseTextFileLogger(this LoggingConfiguration logging, Action<TextFileLoggerConfiguration> textFileLogger = null)
        {
            if (logging == null) throw new ArgumentNullException("logging");

            var configuration = new TextFileLoggerConfiguration();

            if (textFileLogger != null)
                textFileLogger(configuration);

            logging.Application.AddCustomInstaller(new ConfigurationInstaller(configuration));
            logging.Use<TextFileLogger>();

            return logging;
        }

        private class ConfigurationInstaller : IWindsorInstaller
        {
            private readonly TextFileLoggerConfiguration _configuration;

            public ConfigurationInstaller(TextFileLoggerConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void Install(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(
                    Component.For<TextFileLoggerConfiguration>()
                        .UsingFactoryMethod(() => _configuration));
            }
        }
    }
}