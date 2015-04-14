using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Azure.Infrastructure.Castle.Windsor;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure
{
    public class AzureConfiguration
    {
        private readonly List<IWindsorInstaller> _customInstallers;

        public AzureConfiguration()
        {
            _customInstallers = new List<IWindsorInstaller>();
        }

        internal IWindsorInstaller[] CustomInstallers
        {
            get { return _customInstallers.ToArray(); }
        }

        public AzureConfiguration ReplaceArchiveWithBlobStorage(ConnectionString connectionString, string containerName = "archives")
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _customInstallers.Add(new AzureArchiveInstaller(connectionString, containerName));

            return this;
        }

        public AzureConfiguration ReplaceFileSystemWithBlobStorage(ConnectionString connectionString, string containerName = "filesystem")
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _customInstallers.Add(new AzureFileSystemInstaller(connectionString, containerName));

            return this;            
        }
    }
}