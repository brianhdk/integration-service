using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.MongoDB.Infrastructure;
using Vertica.Integration.MongoDB.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.MongoDB
{
    public class MongoDBConfiguration : IInitializable<IWindsorContainer>
    {
		private IWindsorInstaller _defaultConnection;
        private readonly List<IWindsorInstaller> _connections;

        internal MongoDBConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application.Extensibility(extensibility => extensibility.Register(this));

			_connections = new List<IWindsorInstaller>();
        }

        public ApplicationConfiguration Application { get; private set; }

		public MongoDBConfiguration DefaultConnection(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

			_defaultConnection = new MongoDBInstaller(new DefaultConnection(connectionString));

            return this;
        }

		public MongoDBConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            if (connection == null) throw new ArgumentNullException("connection");

			_connections.Add(new MongoDBInstaller<TConnection>(connection));

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
	        container.Install(new ConventionInstaller().AddFromAssemblyOfThis<MongoDBConfiguration>());

	        if (_defaultConnection != null)
		        container.Install();

	        foreach (IWindsorInstaller installer in _connections)
		        container.Install(installer);
        }
    }
}