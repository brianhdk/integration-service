using System;
using System.Reflection;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    internal class MigrationDestination
    {
        public MigrationDestination(DatabaseServer databaseServer, string connectionString, Assembly assembly, string namespaceContainingMigrations)
        {
            if (String.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Value cannot be null or empty.", "connectionString");
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (String.IsNullOrWhiteSpace(namespaceContainingMigrations)) throw new ArgumentException("Value cannot be null or empty.", "namespaceContainingMigrations");

            DatabaseServer = databaseServer;
            ConnectionString = connectionString;
            Assembly = assembly;
            NamespaceContainingMigrations = namespaceContainingMigrations;
        }

        public DatabaseServer DatabaseServer { get; set; }
        public string ConnectionString { get; set; }
        public Assembly Assembly { get; set; }
        public string NamespaceContainingMigrations { get; set; }
    }
}