using System;
using Vertica.Integration.Azure.Infrastructure.BlobStorage;
using Vertica.Integration.Azure.Infrastructure.ServiceBus;

namespace Vertica.Integration.Azure
{
    public class AzureConfiguration
    {
	    private AzureBlobStorageConfiguration _blobStorage;
		private AzureServiceBusConfiguration _serviceBus;

        internal AzureConfiguration(ApplicationConfiguration application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));

			Application = application.Extensibility(extensibility =>
			{
				_blobStorage = extensibility.Register(() => new AzureBlobStorageConfiguration(application));
				_serviceBus = extensibility.Register(() => new AzureServiceBusConfiguration(application));				
			});
        }

        public ApplicationConfiguration Application { get; }
		
        public AzureConfiguration BlobStorage(Action<AzureBlobStorageConfiguration> blobStorage)
        {
	        if (blobStorage == null) throw new ArgumentNullException(nameof(blobStorage));

	        blobStorage(_blobStorage);

            return this;            
        }

		public AzureConfiguration ServiceBus(Action<AzureServiceBusConfiguration> serviceBus)
		{
			if (serviceBus == null) throw new ArgumentNullException(nameof(serviceBus));

			serviceBus(_serviceBus);

			return this;
		}
    }
}