using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration
{
    public class ApplicationConfiguration
    {
        private readonly List<IWindsorInstaller> _customInstallers;

        public ApplicationConfiguration()
        {
            _customInstallers = new List<IWindsorInstaller>();
            DatabaseConnectionStringName = "IntegrationDb";
        }

        public ApplicationConfiguration AddCustomInstaller(IWindsorInstaller installer)
        {
            if (installer == null) throw new ArgumentNullException("installer");

            AddCustomInstallers(installer);

            return this;
        }

        public ApplicationConfiguration AddCustomInstallers(params IWindsorInstaller[] installers)
        {
            _customInstallers.AddRange(installers.EmptyIfNull().SkipNulls());

            return this;
        }

        public IWindsorInstaller[] CustomInstallers
        {
            get { return _customInstallers.ToArray(); }
        }

        public string DatabaseConnectionStringName { get; set; }
    }
}