using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Databases;

namespace Vertica.Integration.Infrastructure.Database
{
    public class DatabaseConfiguration : IInitializable<IWindsorContainer>
    {
        private ConnectionString _databaseConnectionString;

        internal DatabaseConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

            Application = application;
        }

        public ApplicationConfiguration Application { get; private set; }

        public bool IntegrationDbDisabled { get; private set; }

        public ConnectionString ConnectionString
        {
            get
            {
                if (_databaseConnectionString == null)
                    _databaseConnectionString = ConnectionString.FromName("IntegrationDb");

                return _databaseConnectionString;
            }
            set
            {
                IntegrationDbDisabled = value == null;
                _databaseConnectionString = value;
            }
        }

        public DatabaseConfiguration DisableIntegrationDb()
        {
            IntegrationDbDisabled = true;
            return this;
        }

        public DatabaseConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            Application.AddCustomInstaller(new DbInstaller<TConnection>(connection));

            return this;
        }

        public DatabaseConfiguration Change(Action<DatabaseConfiguration> change)
        {
            if (change != null)
                change(this);

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.Install(new DbInstaller(IntegrationDbDisabled ? IntegrationDb.Disabled : new IntegrationDb(ConnectionString)));
        }
    }
}