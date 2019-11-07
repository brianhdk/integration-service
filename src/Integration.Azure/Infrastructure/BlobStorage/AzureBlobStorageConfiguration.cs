using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Azure.Infrastructure.Castle.Windsor;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
	public class AzureBlobStorageConfiguration : IInitializable<ApplicationConfiguration>
	{
		private IWindsorInstaller _defaultConnection;
		private readonly List<IWindsorInstaller> _connections;

		internal AzureBlobStorageConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

		    _connections = new List<IWindsorInstaller>();

		    Application = application;
        }

        public ApplicationConfiguration Application { get; }

		public AzureBlobStorageConfiguration DefaultConnection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			_defaultConnection = new AzureBlobStorageInstaller(new DefaultConnection(connectionString));

			return this;
		}

		public AzureBlobStorageConfiguration DefaultConnection(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_defaultConnection = new AzureBlobStorageInstaller(new DefaultConnection(connection));

			return this;
		}

		public AzureBlobStorageConfiguration AddConnection<TConnection>(TConnection connection)
			where TConnection : Connection
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connections.Add(new AzureBlobStorageInstaller<TConnection>(connection));

			return this;
		}

		public AzureBlobStorageConfiguration ReplaceArchiver(ConnectionString connectionString, string containerName = "archives")
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(containerName));

            _connections.Add(new AzureArchiveInstaller(connectionString, containerName));

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