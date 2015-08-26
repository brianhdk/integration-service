using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Vertica.Integration.Azure.Infrastructure.ServiceBus
{
	public interface IAzureServiceBusClientFactory : IAzureServiceBusClientFactory<DefaultConnection>
	{
	}

    public interface IAzureServiceBusClientFactory<TConnection>
        where TConnection : Connection
    {
	    NamespaceManager CreateNamespaceManager();
		QueueClient CreateQueueClient(string queueName = null);
    }
}