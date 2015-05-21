using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class LoggerConfiguration : IInitializable<IWindsorContainer>
    {
        private Type _logger;

        public LoggerConfiguration()
        {
            _logger = typeof (DefaultLogger);
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.Register(
                Component.For<ILogger>()
                    .ImplementedBy(_logger));
        }

        public void Use<T>() where T : class, ILogger
        {
            _logger = typeof (T);
        }
    }
}