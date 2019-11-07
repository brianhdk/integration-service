using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Elasticsearch.Infrastructure.Clusters.Castle.Windsor
{
	internal class ElasticClusterInstaller : ElasticClusterInstaller<DefaultConnection>
	{
		public ElasticClusterInstaller(DefaultConnection connection)
			: base(connection)
		{
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
			if (container.Kernel.HasComponent(typeof(IElasticClientFactory)))
				throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

			base.Install(container, store);

			container.Register(
				Component.For<IElasticClientFactory>()
					.UsingFactoryMethod((kernel, model, context) =>
						new ElasticClientFactory(kernel.Resolve<IElasticClientFactory<DefaultConnection>>())));
		}
	}

	internal class ElasticClusterInstaller<TConnection> : IWindsorInstaller
        where TConnection : Connection
    {
        private readonly TConnection _connection;

		public ElasticClusterInstaller(TConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IElasticClientFactory<TConnection>>()
                    .UsingFactoryMethod((kernel, model, context) => new ElasticClientFactory<TConnection>(_connection, kernel)));
        }
    }
}