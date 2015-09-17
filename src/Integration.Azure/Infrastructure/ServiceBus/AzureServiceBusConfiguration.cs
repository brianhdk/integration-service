using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Azure.Infrastructure.Castle.Windsor;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.ServiceBus
{
	public class AzureServiceBusConfiguration : IInitializable<IWindsorContainer>
	{
		private IWindsorInstaller _defaultConnection;
		private readonly List<IWindsorInstaller> _installers;

		internal AzureServiceBusConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application;

            _installers = new List<IWindsorInstaller>();
        }

        public ApplicationConfiguration Application { get; private set; }

		public AzureServiceBusConfiguration DefaultConnection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			_defaultConnection = new AzureServiceBusInstaller(new DefaultConnection(connectionString));

			return this;
		}

		public AzureServiceBusConfiguration DefaultConnection(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_defaultConnection = new AzureServiceBusInstaller(new DefaultConnection(connection));

			return this;
		}

		public AzureServiceBusConfiguration AddConnection<TConnection>(TConnection connection)
			where TConnection : Connection
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_installers.Add(new AzureServiceBusInstaller<TConnection>(connection));

			return this;
		}

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
			if (_defaultConnection != null)
				container.Install(_defaultConnection);

            foreach (IWindsorInstaller installer in _installers)
                container.Install(installer);
        }
	}
}