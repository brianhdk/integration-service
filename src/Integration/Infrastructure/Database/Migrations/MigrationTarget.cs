using System;
using System.Reflection;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    internal class MigrationTarget
    {
        public MigrationTarget(DatabaseServer databaseServer, ConnectionString connectionString, Assembly assembly, string namespaceContainingMigrations)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (String.IsNullOrWhiteSpace(namespaceContainingMigrations)) throw new ArgumentException(@"Value cannot be null or empty.", "namespaceContainingMigrations");

            DatabaseServer = databaseServer;
            ConnectionString = connectionString;
            Assembly = assembly;
            NamespaceContainingMigrations = namespaceContainingMigrations;
        }

        public DatabaseServer DatabaseServer { get; private set; }
        public ConnectionString ConnectionString { get; private set; }
        public Assembly Assembly { get; private set; }
        public string NamespaceContainingMigrations { get; private set; }
    }
}