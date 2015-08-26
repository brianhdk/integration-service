using System;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Azure.Infrastructure.ServiceBus;

namespace Vertica.Integration.Azure
{
    public class AzureConfiguration
    {
	    private readonly AzureBlobStorageConfiguration _blobStorage;
		private readonly AzureServiceBusConfiguration _serviceBus;

        internal AzureConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException("application");

			Application = application.Extensibility(extensibility => extensibility.Register(this));

	        _blobStorage = new AzureBlobStorageConfiguration(application);
	        _serviceBus = new AzureServiceBusConfiguration(application);
        }

        public ApplicationConfiguration Application { get; private set; }
		
        public AzureConfiguration BlobStorage(Action<AzureBlobStorageConfiguration> blobStorage)
        {
	        if (blobStorage == null) throw new ArgumentNullException("blobStorage");

	        blobStorage(_blobStorage);

            return this;            
        }

		public AzureConfiguration ServiceBus(Action<AzureServiceBusConfiguration> serviceBus)
		{
			if (serviceBus == null) throw new ArgumentNullException("serviceBus");

			serviceBus(_serviceBus);

			return this;
		}
    }
}