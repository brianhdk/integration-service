using System;
using System.Data;
using System.IO;
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
				: base(connectionString ?? FromFileInCurrentDirectory("IntegrationDb.sqlite"))
		    {
		    }
	    }
    }

	public abstract class SQLiteConnection : Connection
	{
		protected SQLiteConnection(ConnectionString connectionString)
			: base(connectionString)
		{
		}

		protected override IDbConnection GetConnection()
		{
			return new System.Data.SQLite.SQLiteConnection(ConnectionString);
		}

		protected static ConnectionString FromFileInCurrentDirectory(string fileName)
		{
			if (String.IsNullOrWhiteSpace(fileName)) throw new ArgumentException(@"Value cannot be null or empty.", "fileName");

			string file = Path.Combine(Environment.CurrentDirectory, fileName);

			return ConnectionString.FromText(String.Format("Data Source={0}", file));
		}
	}
}