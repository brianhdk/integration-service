using System;
using System.Reflection;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
	public class MigrationDb : IEquatable<MigrationDb>
	{
        public MigrationDb(DatabaseServer databaseServer, ConnectionString connectionString, Assembly assembly, string namespaceContainingMigrations)
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

		public MigrationDb CopyTo(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			return new MigrationDb(DatabaseServer, ConnectionString, type.Assembly, type.Namespace);
		}

		public bool Equals(MigrationDb other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return 
				Equals(Assembly, other.Assembly) && 
				String.Equals(NamespaceContainingMigrations, other.NamespaceContainingMigrations) && 
				Equals(ConnectionString, other.ConnectionString);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((MigrationDb) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Assembly != null ? Assembly.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (NamespaceContainingMigrations != null ? NamespaceContainingMigrations.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (ConnectionString != null ? ConnectionString.GetHashCode() : 0);
				return hashCode;
			}
		}
    }
}