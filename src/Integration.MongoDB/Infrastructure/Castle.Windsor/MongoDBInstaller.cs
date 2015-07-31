using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.MongoDB.Infrastructure.Castle.Windsor
{
    internal class MongoDBInstaller : MongoDBInstaller<DefaultConnection>
    {
        public MongoDBInstaller(DefaultConnection connection)
            : base(connection)
        {
        }

        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            if (container.Kernel.HasComponent(typeof(IMongoDBClientFactory)))
                throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

            base.Install(container, store);

            container.Register(
                Component.For<IMongoDBClientFactory>()
                    .UsingFactoryMethod(kernel => 
						new MongoDBClientFactory(kernel.Resolve<IMongoDBClientFactory<DefaultConnection>>())));
        }
    }

    internal class MongoDBInstaller<TConnection> : IWindsorInstaller
        where TConnection : Connection
    {
        private readonly TConnection _connection;

        public MongoDBInstaller(TConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
        }

        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IMongoDBClientFactory<TConnection>>()
                    .UsingFactoryMethod(() => new MongoDBClientFactory<TConnection>(_connection)));
        }
    }
}