using System;
using System.Text.RegularExpressions;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.Infrastructure.Database
{
    public class IntegrationDatabaseConfiguration : IInitializable<IWindsorContainer>
    {
        private readonly Configuration _configuration;
        private DefaultConnection _defaultConnection;

        internal IntegrationDatabaseConfiguration(DatabaseConfiguration database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            _configuration = new Configuration();

            Database = database;
        }

        public DatabaseConfiguration Database { get; }

        [Obsolete("Property will be removed.")]
        public bool Disabled => _configuration.Disabled;

        public IntegrationDatabaseConfiguration Disable()
        {
            _configuration.Disabled = true;

            return this;
        }

        public IntegrationDatabaseConfiguration ChangeDatabaseServer(DatabaseServer databaseServer)
        {
            _configuration.DatabaseServer = databaseServer;

            return this;
        }

        public IntegrationDatabaseConfiguration DisableCheckExistsAndCreateDatabaseIfNotFound()
        {
            _configuration.CheckExistsAndCreateDatabaseIfNotFound = false;

            return this;
        }

        /// <summary>
        ///     Few rules about the prefix, only chars, numbers and the following symbols are supported: _-.
        ///     Also the length of the string cannot not be longer than 20.
        /// </summary>
        public IntegrationDatabaseConfiguration PrefixTables(string prefix)
        {
            _configuration.PrefixTables(prefix);

            return this;
        }

        public IntegrationDatabaseConfiguration Change(Action<IntegrationDatabaseConfiguration> change)
        {
            change?.Invoke(this);

            return this;
        }

        /// <summary>
        ///     Specifies the connection string for the IntegrationDb.
        /// </summary>
        public IntegrationDatabaseConfiguration Connection(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            _defaultConnection = new DefaultConnection(connectionString);

            return this;
        }

        /// <summary>
        ///     Specifies a specific connection instance for the IntegrationDb.
        /// </summary>
        public IntegrationDatabaseConfiguration Connection(Connection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _defaultConnection = new DefaultConnection(connection);

            return this;
        }

        void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
            container.RegisterInstance<IIntegrationDatabaseConfiguration>(_configuration, x => x.LifestyleSingleton());

            container.Install(new DbInstaller(_configuration.Disabled
                ? DefaultConnection.Disabled
                : _defaultConnection ?? new DefaultConnection(ConnectionString.FromName("IntegrationDb"))));
        }

        private class Configuration : IIntegrationDatabaseConfiguration
        {
            public Configuration()
            {
                DatabaseServer = DatabaseServer.SqlServer2014;
                CheckExistsAndCreateDatabaseIfNotFound = true;
            }

            private string TablesPrefix { get; set; }

            public bool Disabled { get; set; }

            public string TableName(IntegrationDbTable table)
            {
                return string.Concat(TablesPrefix, table);
            }

            public DatabaseServer DatabaseServer { get; set; }
            public bool CheckExistsAndCreateDatabaseIfNotFound { get; set; }

            public void PrefixTables(string prefix)
            {
                if (prefix != null)
                {
                    prefix = prefix.Trim();

                    const string pattern = @"^[A-Za-z0-9|_|-|\.]{0,20}$";

                    if (!Regex.IsMatch(prefix, pattern, RegexOptions.IgnoreCase))
                        throw new ArgumentOutOfRangeException(nameof(prefix),
                            $"Prefix does not comply with the following pattern: {pattern}.");
                }

                TablesPrefix = prefix;
            }
        }
    }
}