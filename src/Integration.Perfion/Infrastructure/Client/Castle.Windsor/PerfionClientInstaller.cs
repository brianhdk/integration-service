using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Perfion.Infrastructure.Client.Castle.Windsor
{
    internal class PerfionClientInstaller<TConnection> : IWindsorInstaller
        where TConnection : Connection
    {
        private readonly TConnection _connection;
        private readonly IPerfionClientConfiguration _configuration;

        public PerfionClientInstaller(TConnection connection, IPerfionClientConfiguration configuration)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _connection = connection;
            _configuration = configuration;
        }

        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IPerfionClientFactory<TConnection>>()
                    .UsingFactoryMethod((kernel, model, context) => new PerfionClientFactory<TConnection>(_connection, _configuration, kernel)));
        }
    }

    internal class PerfionClientInstaller : PerfionClientInstaller<DefaultConnection>
    {
        public PerfionClientInstaller(DefaultConnection connection, IPerfionClientConfiguration configuration)
            : base(connection, configuration)
        {
        }

        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (container.Kernel.HasComponent(typeof(IPerfionClientFactory)))
                throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

            base.Install(container, store);

            container.Register(
                Component.For<IPerfionClientFactory>()
                    .UsingFactoryMethod((kernel, model, context) => new PerfionClientFactory(kernel.Resolve<IPerfionClientFactory<DefaultConnection>>())));

            container.Register(
                Component.For<IPerfionClient>()
                    .UsingFactoryMethod((kernel, model, context) => kernel.Resolve<IPerfionClientFactory>().Client));
        }
    }
}