using System;
using System.Collections.Generic;
using System.Configuration;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class MigrationConfiguration
    {
        private readonly List<MigrationDestination> _customDestinations;
        private bool _locked;

        internal MigrationConfiguration()
        {
            _customDestinations = new List<MigrationDestination>();

            ChangeIntegrationDbDatabaseServer(DatabaseServer.SqlServer2014);
        }

        public MigrationConfiguration ChangeIntegrationDbDatabaseServer(DatabaseServer db)
        {
            AssertNotLocked();

            IntegrationDbDatabaseServer = db;

            return this;
        }

        public MigrationConfiguration IncludeFromNamespaceOfThis<T>(DatabaseServer db, string connectionStringName)
        {
            if (String.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentException(@"Value cannot be null or empty.", "connectionStringName");

            AssertNotLocked();

            ConnectionStringSettings connectionString =
                ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionString == null)
                throw new ArgumentException(
                    String.Format("No ConnectionString found with name '{0}'.", connectionStringName));

            _customDestinations.Add(new MigrationDestination(
                db,
                connectionString.ConnectionString,
                typeof(T).Assembly,
                typeof(T).Namespace));

            return this;
        }

        private void AssertNotLocked()
        {
            if (_locked)
                throw new InvalidOperationException("You can only modify the state of this configuration object when calling ApplicationContext.Create().");
        }

        internal MigrationConfiguration Lock()
        {
            _locked = true;

            return this;
        }

        internal DatabaseServer IntegrationDbDatabaseServer { get; private set; }

        internal MigrationDestination[] CustomDestinations
        {
            get { return _customDestinations.ToArray(); }
        }
    }
}