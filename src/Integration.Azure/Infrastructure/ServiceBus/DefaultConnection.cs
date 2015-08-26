using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.ServiceBus
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

		protected internal override NamespaceManager CreateNamespaceManager()
		{
			if (_connection != null)
				return _connection.CreateNamespaceManager();

			return base.CreateNamespaceManager();
		}

		protected internal override QueueClient CreateQueueClient(string queueName = null)
		{
			if (_connection != null)
				return _connection.CreateQueueClient(queueName);

			return base.CreateQueueClient(queueName);
		}
	}
}