using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Databases;

namespace Vertica.Integration.Infrastructure.Database
{
    public class DatabaseConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly ApplicationConfiguration _configuration;
        private ConnectionString _databaseConnectionString;
        private bool _disabled;

        internal DatabaseConfiguration(ApplicationConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            _configuration = configuration;
        }

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
                _disabled = value == null;
                _databaseConnectionString = value;
            }
        }

        public DatabaseConfiguration DisableIntegrationDb()
        {
            _disabled = true;
            return this;
        }

        public DatabaseConfiguration AddConnection<TConnection>(TConnection connection)
            where TConnection : Connection
        {
            _configuration.AddCustomInstaller(new DbInstaller<TConnection>(connection));

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
            container.Install(new DbInstaller(_disabled ? IntegrationDb.Disabled : new IntegrationDb(ConnectionString)));
        }
    }
}