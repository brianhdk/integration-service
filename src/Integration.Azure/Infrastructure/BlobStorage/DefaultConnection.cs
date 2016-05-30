using System;
using Castle.MicroKernel;
using Microsoft.WindowsAzure.Storage.Analytics;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
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
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;
		}

		[Obsolete("Use CreateBlobClient method")]
		protected internal override CloudBlobClient Create(IKernel kernel)
		{
			if (_connection != null)
				return _connection.Create(kernel);

			return base.Create(kernel);
		}

		protected internal override CloudBlobClient CreateBlobClient(IKernel kernel)
		{
			if (_connection != null)
				return _connection.CreateBlobClient(kernel);

			return base.CreateBlobClient(kernel);
		}

		protected internal override CloudQueueClient CreateQueueClient(IKernel kernel)
		{
			if (_connection != null)
				return _connection.CreateQueueClient(kernel);

			return base.CreateQueueClient(kernel);
		}

		protected internal override CloudTableClient CreateTableClient(IKernel kernel)
		{
			if (_connection != null)
				return _connection.CreateTableClient(kernel);

			return base.CreateTableClient(kernel);
		}

		protected internal override CloudAnalyticsClient CreateAnalyticsClient(IKernel kernel)
		{
			if (_connection != null)
				return _connection.CreateAnalyticsClient(kernel);

			return base.CreateAnalyticsClient(kernel);
		}

		protected internal override CloudFileClient CreateFileClient(IKernel kernel)
		{
			if (_connection != null)
				return _connection.CreateFileClient(kernel);

			return base.CreateFileClient(kernel);
		}
	}
}