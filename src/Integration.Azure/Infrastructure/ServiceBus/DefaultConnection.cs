using System;
using Castle.MicroKernel;
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

		protected internal override NamespaceManager CreateNamespaceManager(IKernel kernel)
		{
			if (_connection != null)
				return _connection.CreateNamespaceManager(kernel);

			return base.CreateNamespaceManager(kernel);
		}

		protected internal override QueueClient CreateQueueClient(IKernel kernel, string queueName = null)
		{
			if (_connection != null)
				return _connection.CreateQueueClient(kernel, queueName);

			return base.CreateQueueClient(kernel, queueName);
		}
	}
}