using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Integration.Azure.Infrastructure.Castle.Windsor;

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

        public AzureConfiguration ReplaceArchiverWithBlobStorage(string connectionStringName, string containerName = "archives")
        {
            if (String.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentException(@"Value cannot be null or empty.", "connectionStringName");
            if (String.IsNullOrWhiteSpace(containerName)) throw new ArgumentException(@"Value cannot be null or empty.", "containerName");

            _customInstallers.Add(new AzureArchiverInstaller(connectionStringName, containerName));

            return this;
        }
    }
}