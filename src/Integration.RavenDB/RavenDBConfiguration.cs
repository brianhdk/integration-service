using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.RavenDB.Infrastructure;
using Vertica.Integration.RavenDB.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.RavenDB
{
    public class RavenDbConfiguration : IInitializable<ApplicationConfiguration>
    {
		private IWindsorInstaller _defaultConnection;
        private readonly List<IWindsorInstaller> _connections;

		internal RavenDbConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application;

			_connections = new List<IWindsorInstaller>();
        }

        public ApplicationConfiguration Application { get; private set; }

	    public RavenDbConfiguration DefaultConnection(ConnectionString connectionString)
	    {
		    if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

		    _defaultConnection = new RavenDbInstaller(new DefaultConnection(connectionString));

			return this;
	    }

	    public RavenDbConfiguration DefaultConnection(Connection connection)
        {
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_defaultConnection = new RavenDbInstaller(new DefaultConnection(connection));

            return this;
        }

		public RavenDbConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connections.Add(new RavenDbInstaller<TConnection>(connection));

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