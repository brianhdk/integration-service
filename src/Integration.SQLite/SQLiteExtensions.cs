using System;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.SQLite
{
	public static class SQLiteExtensions
	{
		public static DatabaseConfiguration SQLite(this DatabaseConfiguration database, Action<SQLiteConfiguration> sqlite = null)
		{
			if (database == null) throw new ArgumentNullException("database");

			database.Application.Extensibility(extensibility =>
			{
				var configuration = extensibility.Register(() => new SQLiteConfiguration(database));

				if (sqlite != null)
					sqlite(configuration);
			});

			return database;
		}
	}
}