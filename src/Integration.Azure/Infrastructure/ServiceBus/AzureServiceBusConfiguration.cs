using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Azure.Infrastructure.Castle.Windsor;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.ServiceBus
{
	public class AzureServiceBusConfiguration : IInitializable<ApplicationConfiguration>
	{
		private IWindsorInstaller _defaultConnection;
		private readonly List<IWindsorInstaller> _connections;

		internal AzureServiceBusConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

		    _connections = new List<IWindsorInstaller>();

		    Application = application;
        }

        public ApplicationConfiguration Application { get; }

		public AzureServiceBusConfiguration DefaultConnection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			_defaultConnection = new AzureServiceBusInstaller(new DefaultConnection(connectionString));

			return this;
		}

		public AzureServiceBusConfiguration DefaultConnection(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_defaultConnection = new AzureServiceBusInstaller(new DefaultConnection(connection));

			return this;
		}

		public AzureServiceBusConfiguration AddConnection<TConnection>(TConnection connection)
			where TConnection : Connection
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connections.Add(new AzureServiceBusInstaller<TConnection>(connection));

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