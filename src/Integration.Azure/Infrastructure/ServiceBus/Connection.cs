using System;
using Castle.MicroKernel;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Azure.Infrastructure.ServiceBus
{
	public abstract class Connection
	{
        protected Connection(ConnectionString connectionString)
		{
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			ConnectionString = connectionString;
		}

        protected internal ConnectionString ConnectionString { get; private set; }

		protected internal virtual NamespaceManager CreateNamespaceManager(IKernel kernel)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			return NamespaceManager.CreateFromConnectionString(ConnectionString);
		}

		protected internal virtual QueueClient CreateQueueClient(IKernel kernel, string queueName = null)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			if (!string.IsNullOrWhiteSpace(queueName))
				return QueueClient.CreateFromConnectionString(ConnectionString, queueName);

			return QueueClient.CreateFromConnectionString(ConnectionString);
		}
	}
}