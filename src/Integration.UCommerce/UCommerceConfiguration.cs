using System;
using FluentMigrator;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.UCommerce.Database;

namespace Vertica.Integration.UCommerce
{
	public class UCommerceConfiguration : IInitializable<ApplicationConfiguration>
	{
	    private UCommerceDb _connection;
	    private Action<ConnectionString> _migration;

	    internal UCommerceConfiguration(ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

	        Application = application;
		}

		public UCommerceConfiguration Change(Action<UCommerceConfiguration> change)
		{
			change?.Invoke(this);

			return this;
		}

		public ApplicationConfiguration Application { get; }

        public UCommerceConfiguration Connection(ConnectionString connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            return Connection(new DefaultConnection(connectionString));
        }

        public UCommerceConfiguration Connection<TConnection>(TConnection connection)
            where TConnection : UCommerceDb
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _connection = connection;

            return this;
        }

        internal void AddMigrationFromNamespaceOfThis<T>(DatabaseServer db, string identifyingName)
            where T : Migration
        {
            _migration = connectionString =>
            {
                Application.Migration(migration => migration
                    .AddFromNamespaceOfThis<T>(db, connectionString, identifyingName));
            };
        }

        void IInitializable<ApplicationConfiguration>.Initialized(ApplicationConfiguration application)
        {
            if (_connection == null)
                Connection(ConnectionString.FromName("uCommerceDb"));

            if (_connection != null)
            {
                Application.Database(database => database.AddConnection(_connection));

                _migration?.Invoke(_connection.ConnectionString);
            }
        }
	}
}