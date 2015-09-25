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
    public class MongoDbConfiguration : IInitializable<IWindsorContainer>
    {
		private IWindsorInstaller _defaultConnection;
        private readonly List<IWindsorInstaller> _connections;

        internal MongoDbConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application;

			_connections = new List<IWindsorInstaller>();
        }

        public ApplicationConfiguration Application { get; private set; }

		public MongoDbConfiguration DefaultConnection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			_defaultConnection = new MongoDbInstaller(new DefaultConnection(connectionString));

			return this;
		}

		public MongoDbConfiguration DefaultConnection(Connection connection)
        {
			if (connection == null) throw new ArgumentNullException("connection");

			_defaultConnection = new MongoDbInstaller(new DefaultConnection(connection));

            return this;
        }

		public MongoDbConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            if (connection == null) throw new ArgumentNullException("connection");

			_connections.Add(new MongoDbInstaller<TConnection>(connection));

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
	        container.Install(Install.ByConvention.AddFromAssemblyOfThis<MongoDbConfiguration>());

	        if (_defaultConnection != null)
		        container.Install(_defaultConnection);

	        foreach (IWindsorInstaller installer in _connections)
		        container.Install(installer);
        }
    }
}