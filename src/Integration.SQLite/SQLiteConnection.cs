using System;
using System.Data;
using System.IO;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.SQLite
{
	public abstract class SQLiteConnection : Connection
	{
		protected SQLiteConnection(ConnectionString connectionString)
			: base(connectionString)
		{
		}

		protected override IDbConnection GetConnection(IKernel kernel)
		{
			return new System.Data.SQLite.SQLiteConnection(ConnectionString);
		}

		public static ConnectionString FromCurrentDirectory(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(filePath));

			filePath = Path.Combine(Environment.CurrentDirectory, filePath);

			DirectoryInfo directoryInfo = new FileInfo(filePath).Directory;

			if (directoryInfo != null && !directoryInfo.Exists)
				directoryInfo.Create();

			return ConnectionString.FromText($"Data Source={filePath}");
		}
	}
}