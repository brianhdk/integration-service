using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Serilog;

namespace Vertica.Integration.Serilog.Infrastructure.Castle.Windsor
{
    internal class SerilogLoggerInstaller<TLogger> : IWindsorInstaller
        where TLogger : Logger
    {
        private readonly TLogger _logger;

        public SerilogLoggerInstaller(TLogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<ILoggerFactory<TLogger>>()
                    .UsingFactoryMethod((kernel, model, context) => new LoggerFactory<TLogger>(_logger, kernel)));
        }
    }

    internal class SerilogLoggerInstaller : SerilogLoggerInstaller<DefaultLogger>
    {
        private readonly DefaultLogger _logger;

        public SerilogLoggerInstaller(DefaultLogger logger)
            : base(logger)
        {
            _logger = logger;
        }

        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (container.Kernel.HasComponent(typeof(ILogger)))
                throw new InvalidOperationException("Only one DefaultLogger can be installed. Use the generic installer for additional instances.");

            base.Install(container, store);

            container.Register(
                Component.For<ILoggerFactory>()
                    .UsingFactoryMethod((kernel, model, context) => new LoggerFactory(kernel.Resolve<ILoggerFactory<DefaultLogger>>())));

            container.Register(
                Component.For<ILogger>()
                    .UsingFactoryMethod((kernel, model, context) => kernel.Resolve<ILoggerFactory>().Logger)
                    .OnDestroy(x =>
                    {
                        if (_logger.SetGlobalStaticLogger)
                            Log.CloseAndFlush();
                    }));

            if (_logger.SetGlobalStaticLogger)
                Log.Logger = container.Resolve<ILogger>();
        }
    }
}