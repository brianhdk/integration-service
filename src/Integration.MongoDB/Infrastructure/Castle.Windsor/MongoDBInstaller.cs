using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.MongoDB.Infrastructure.Castle.Windsor
{
    internal class MongoDbInstaller : MongoDbInstaller<DefaultConnection>
    {
        public MongoDbInstaller(DefaultConnection connection)
            : base(connection)
        {
        }

        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (container.Kernel.HasComponent(typeof(IMongoDbClientFactory)))
                throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

            base.Install(container, store);

            container.Register(
                Component.For<IMongoDbClientFactory>()
                    .UsingFactoryMethod(kernel => 
						new MongoDbClientFactory(kernel.Resolve<IMongoDbClientFactory<DefaultConnection>>())));
        }
    }

    internal class MongoDbInstaller<TConnection> : IWindsorInstaller
        where TConnection : Connection
    {
        private readonly TConnection _connection;

        public MongoDbInstaller(TConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
        }

        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IMongoDbClientFactory<TConnection>>()
                    .UsingFactoryMethod(kernel => new MongoDBClientFactory<TConnection>(_connection, kernel)));
        }
    }
}