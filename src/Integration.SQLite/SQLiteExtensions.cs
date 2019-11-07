using System;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.SQLite
{
	public static class SQLiteExtensions
	{
		public static DatabaseConfiguration SQLite(this DatabaseConfiguration database, Action<SQLiteConfiguration> sqlite)
		{
			if (database == null) throw new ArgumentNullException(nameof(database));
			if (sqlite == null) throw new ArgumentNullException(nameof(sqlite));

			database.Application.Extensibility(extensibility =>
			{
				SQLiteConfiguration configuration = extensibility.Register(() => new SQLiteConfiguration(database));

				sqlite(configuration);
			});

			return database;
		}
	}
}