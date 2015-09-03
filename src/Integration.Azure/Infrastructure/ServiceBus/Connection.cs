using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.ServiceBus
{
	public abstract class Connection
	{
        protected Connection(ConnectionString connectionString)
		{
            if (connectionString == null) throw new ArgumentNullException("connectionString");

			ConnectionString = connectionString;
		}

        protected internal ConnectionString ConnectionString { get; private set; }

		protected internal virtual NamespaceManager CreateNamespaceManager()
		{
			return NamespaceManager.CreateFromConnectionString(ConnectionString);
		}

		protected internal virtual QueueClient CreateQueueClient(string queueName = null)
		{
			if (!String.IsNullOrWhiteSpace(queueName))
				return QueueClient.CreateFromConnectionString(ConnectionString, queueName);

			return QueueClient.CreateFromConnectionString(ConnectionString);
		}
	}
}