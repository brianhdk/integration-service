using System;
using Castle.MicroKernel;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Vertica.Integration.Azure.Infrastructure.ServiceBus
{
    internal class AzureServiceBusClientFactory<TConnection> : IAzureServiceBusClientFactory<TConnection>
        where TConnection : Connection
    {
	    private readonly TConnection _connection;
	    private readonly IKernel _kernel;

	    public AzureServiceBusClientFactory(TConnection connection, IKernel kernel)
	    {
		    _connection = connection;
		    _kernel = kernel;
	    }

	    public NamespaceManager CreateNamespaceManager()
	    {
		    return _connection.CreateNamespaceManager(_kernel);
	    }

	    public QueueClient CreateQueueClient(string queueName = null)
	    {
		    return _connection.CreateQueueClient(_kernel, queueName);
	    }
    }

	internal class AzureServiceBusClientFactory : IAzureServiceBusClientFactory
	{
		private readonly IAzureServiceBusClientFactory<DefaultConnection> _decoree;

		public AzureServiceBusClientFactory(IAzureServiceBusClientFactory<DefaultConnection> decoree)
		{
			if (decoree == null) throw new ArgumentNullException("decoree");

			_decoree = decoree;
		}

		public NamespaceManager CreateNamespaceManager()
		{
			return _decoree.CreateNamespaceManager();
		}

		public QueueClient CreateQueueClient(string queueName = null)
		{
			return _decoree.CreateQueueClient(queueName);
		}
	}
}