using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Database
{
    public class IntegrationDatabaseConfiguration : IIntegrationDatabaseConfiguration, IInitializable<IWindsorContainer>
    {
        private DefaultConnection _defaultConnection;

        internal IntegrationDatabaseConfiguration(DatabaseConfiguration database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            Database = database;
        }

        public DatabaseConfiguration Database { get; }

        public bool Disabled { get; private set; }
        //public string TablePrefix { get; private set; }

        public IntegrationDatabaseConfiguration Disable()
        {
            Disabled = true;

            return this;
        }

        //public IntegrationDatabaseConfiguration PrefixTables(string prefix)
        //{
        //    if (prefix == null) throw new ArgumentNullException(nameof(prefix));

        //    TablePrefix = prefix;

        //    return this;
        //}

        public IntegrationDatabaseConfiguration Change(Action<IntegrationDatabaseConfiguration> change)
        {
            change?.Invoke(this);

            return this;
        }

        /// <summary>
        /// Specifies the connection string for the IntegrationDb.
        /// </summary>
        public IntegrationDatabaseConfiguration Connection(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            _defaultConnection = new DefaultConnection(connectionString);

            return this;
        }

        /// <summary>
        /// Specifies a specific connection instance for the IntegrationDb.
        /// </summary>
        public IntegrationDatabaseConfiguration Connection(Connection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _defaultConnection = new DefaultConnection(connection);

            return this;
        }
        
        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.RegisterInstance<IIntegrationDatabaseConfiguration>(this, x => x.LifestyleSingleton());

            container.Install(new DbInstaller(Disabled ?
                DefaultConnection.Disabled :
                _defaultConnection ?? new DefaultConnection(ConnectionString.FromName("IntegrationDb"))));
        }
    }
}