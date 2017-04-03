using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Redis.Infrastructure.Client.Castle.Windsor
{
    internal class RedisClientInstaller<TConnection> : IWindsorInstaller
        where TConnection : Connection
    {
        private readonly TConnection _connection;

        public RedisClientInstaller(TConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _connection = connection;
        }

        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IRedisClientFactory<TConnection>>()
                    .UsingFactoryMethod(kernel => new RedisClientFactory<TConnection>(_connection, kernel)));
        }
    }

    internal class RedisClientInstaller : RedisClientInstaller<DefaultConnection>
    {
        public RedisClientInstaller(DefaultConnection connection)
            : base(connection)
        {
        }

        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (container.Kernel.HasComponent(typeof(IRedisClientFactory)))
                throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

            base.Install(container, store);

            container.Register(
                Component.For<IRedisClientFactory>()
                    .UsingFactoryMethod(kernel =>
                        new RedisClientFactory(kernel.Resolve<IRedisClientFactory<DefaultConnection>>())));
        }
    }
}