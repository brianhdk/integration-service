using System;
using Vertica.Integration.Azure;
using Vertica.Integration.Logging.Kibana.Infrastructure;

namespace Vertica.Integration.Logging.Kibana
{
    public static class KibanaExtensions
    {
        public static ApplicationConfiguration UseKibana(this ApplicationConfiguration builder, Action<KibanaConfiguration> kibana = null)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            var configuration = new KibanaConfiguration();

            if (kibana != null)
                kibana(configuration);

	        builder.UseAzure(azure => azure.BlobStorage(blobStorage =>
	        {
		        blobStorage.AddConnection(new KibanaConnection(configuration.AzureBlobStorageConnectionString));
	        }));

            return builder;
        }
    }
}