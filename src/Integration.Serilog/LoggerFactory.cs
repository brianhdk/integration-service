using System;
using Castle.MicroKernel;
using Serilog;
using Vertica.Integration.Serilog.Infrastructure;

namespace Vertica.Integration.Serilog
{
    internal class LoggerFactory<TLogger> : ILoggerFactory<TLogger>
        where TLogger : Logger
    {
        private readonly Lazy<ILogger> _logger;

        public LoggerFactory(TLogger logger, IKernel kernel)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            _logger = new Lazy<ILogger>(() => logger.Create(kernel));
        }

        public ILogger Logger => _logger.Value;
    }

    internal class LoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory<DefaultLogger> _decoree;

        public LoggerFactory(ILoggerFactory<DefaultLogger> decoree)
        {
            if (decoree == null) throw new ArgumentNullException(nameof(decoree));

            _decoree = decoree;
        }

        public ILogger Logger => _decoree.Logger;
    }
}