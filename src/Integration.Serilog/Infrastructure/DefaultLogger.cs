using System;
using Castle.MicroKernel;
using Serilog;

namespace Vertica.Integration.Serilog.Infrastructure
{
    public sealed class DefaultLogger : Logger
    {
        private readonly Logger _logger;

        internal DefaultLogger(Logger logger, bool setGlobalStaticLogger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _logger = logger;
            SetGlobalStaticLogger = setGlobalStaticLogger;
        }

        internal DefaultLogger(Func<IKernel, ILogger> factory, bool setGlobalStaticLogger)
            : this(new FactoryBasedLogger(factory), setGlobalStaticLogger)
        {
        }

        protected internal override ILogger Create(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            return _logger.Create(kernel);
        }

        internal bool SetGlobalStaticLogger { get; }

        private class FactoryBasedLogger : Logger
        {
            private readonly Func<IKernel, ILogger> _factory;

            public FactoryBasedLogger(Func<IKernel, ILogger> factory)
            {
                if (factory == null) throw new ArgumentNullException(nameof(factory));

                _factory = factory;
            }

            protected internal override ILogger Create(IKernel kernel)
            {
                if (kernel == null) throw new ArgumentNullException(nameof(kernel));

                return _factory(kernel);
            }
        }
    }
}