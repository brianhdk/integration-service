using System;
using Microsoft.WindowsAzure.Storage.Blob;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.BlobStorage
{
	public sealed class DefaultConnection : Connection
	{
		private readonly Connection _connection;

		internal DefaultConnection(ConnectionString connectionString)
			: base(connectionString)
		{
		}

		internal DefaultConnection(Connection connection)
			: base(connection.ConnectionString)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		protected internal override CloudBlobClient Create()
		{
			if (_connection != null)
				return _connection.Create();

			return base.Create();
		}
	}
}