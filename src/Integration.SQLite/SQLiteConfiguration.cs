using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Vertica.Integration.SQLite
{
    public class SQLiteConfiguration : IInitializable<IWindsorContainer>
    {
        internal SQLiteConfiguration(DatabaseConfiguration database)
        {
            if (database == null) throw new ArgumentNullException("database");

			Database = database;
        }

		public DatabaseConfiguration Database { get; private set; }

	    public SQLiteConfiguration UseForIntegrationDb(ConnectionString connectionString = null)
	    {
			return UseForIntegrationDb(new IntegrationDb(connectionString));
	    }

	    public SQLiteConfiguration UseForIntegrationDb(SQLiteConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

		    Database.IntegrationDb(connection);

			Database.Application.Migration(migration => migration
				.ChangeIntegrationDbDatabaseServer(DatabaseServer.Sqlite)
				.DisableCheckExistsAndCreateIntegrationDbIfNotFound());

			return this;
		}

	    public SQLiteConfiguration AddConnection<TConnection>(TConnection connection)
			where TConnection : SQLiteConnection
		{
			Database.AddConnection(connection);

			return this;
		}

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
	    {
	    }

	    private class IntegrationDb : SQLiteConnection
	    {
		    public IntegrationDb(ConnectionString connectionString)
				: base(connectionString ?? FromCurrentDirectory("IntegrationDb.sqlite"))
		    {
		    }
	    }
    }
}