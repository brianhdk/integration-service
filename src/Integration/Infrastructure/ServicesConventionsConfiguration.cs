using System;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Parsing;

namespace Vertica.Integration.Infrastructure
{
    public class ServicesConventionsConfiguration
    {
        private readonly ConventionInstaller _installer;

        internal ServicesConventionsConfiguration(ServicesConfiguration services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            _installer = new ConventionInstaller();

            // Add own assembly
            AddFromAssemblyOfThis<ServicesConventionsConfiguration>()
                .Ignore<IApplicationContext>()
                .Ignore<ITarget>()
                .Ignore<CsvRow.ICsvRowBuilder>();

            Services = services
                .Advanced(advanced => advanced
                    .Install(_installer));
        }

        public ServicesConventionsConfiguration AddFromAssemblyOfThis<T>()
        {
            _installer.AddFromAssemblyOfThis<T>();

            return this;
        }

        public ServicesConventionsConfiguration Ignore<T>()
        {
            _installer.Ignore<T>();

            return this;
        }

        public ServicesConfiguration Services { get; }
    }
}