using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Azure.Infrastructure.Castle.Windsor;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure
{
    public class AzureConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly List<IWindsorInstaller> _installers;

        internal AzureConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application.Extensibility(extensibility => extensibility.Register(this));

            _installers = new List<IWindsorInstaller>();
        }

        public ApplicationConfiguration Application { get; private set; }

        public AzureConfiguration ReplaceArchiveWithBlobStorage(ConnectionString connectionString, string containerName = "archives")
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _installers.Add(new AzureArchiveInstaller(connectionString, containerName));

            return this;
        }

        public AzureConfiguration ReplaceFileSystemWithBlobStorage(ConnectionString connectionString, string containerName = "filesystem")
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _installers.Add(new AzureFileSystemInstaller(connectionString, containerName));

            return this;            
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            foreach (IWindsorInstaller installer in _installers)
                container.Install(installer);
        }
    }
}