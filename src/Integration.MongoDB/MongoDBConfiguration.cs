using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.MongoDB.Infrastructure;
using Vertica.Integration.MongoDB.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.MongoDB
{
    public class MongoDbConfiguration : IInitializable<ApplicationConfiguration>
    {
		private IWindsorInstaller _defaultConnection;
        private readonly List<IWindsorInstaller> _connections;

        internal MongoDbConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

            Application = application
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<MongoDbConfiguration>()));

            _connections = new List<IWindsorInstaller>();
        }

        public ApplicationConfiguration Application { get; }

		public MongoDbConfiguration DefaultConnection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			_defaultConnection = new MongoDbInstaller(new DefaultConnection(connectionString));

			return this;
		}

		public MongoDbConfiguration DefaultConnection(Connection connection)
        {
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_defaultConnection = new MongoDbInstaller(new DefaultConnection(connection));

            return this;
        }

		public MongoDbConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connections.Add(new MongoDbInstaller<TConnection>(connection));

            return this;
        }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            Application.Services(services => services.Advanced(advanced =>
            {
                if (_defaultConnection != null)
                    advanced.Install(_defaultConnection);

                advanced.Install(_connections.ToArray());
            }));
        }
    }
}