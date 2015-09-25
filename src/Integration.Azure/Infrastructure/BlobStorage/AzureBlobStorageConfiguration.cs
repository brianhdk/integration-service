using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Azure.Infrastructure.Castle.Windsor;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
	public class AzureBlobStorageConfiguration : IInitializable<IWindsorContainer>
	{
		private IWindsorInstaller _defaultConnection;
		private readonly List<IWindsorInstaller> _installers;

		internal AzureBlobStorageConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application;

            _installers = new List<IWindsorInstaller>();
        }

        public ApplicationConfiguration Application { get; private set; }

		public AzureBlobStorageConfiguration DefaultConnection(ConnectionString connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException("connectionString");

			_defaultConnection = new AzureBlobStorageInstaller(new DefaultConnection(connectionString));

			return this;
		}

		public AzureBlobStorageConfiguration DefaultConnection(Connection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_defaultConnection = new AzureBlobStorageInstaller(new DefaultConnection(connection));

			return this;
		}

		public AzureBlobStorageConfiguration AddConnection<TConnection>(TConnection connection)
			where TConnection : Connection
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_installers.Add(new AzureBlobStorageInstaller<TConnection>(connection));

			return this;
		}

		public AzureBlobStorageConfiguration ReplaceArchiver(ConnectionString connectionString, string containerName = "archives")
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _installers.Add(new AzureArchiveInstaller(connectionString, containerName));

            return this;
        }

		// TODO: Implement this
		internal AzureBlobStorageConfiguration ReplaceFileSystem(ConnectionString connectionString, string containerName = "filesystem")
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _installers.Add(new AzureFileSystemInstaller(connectionString, containerName));

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