using System;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Logging.Kibana
{
    public class KibanaConfiguration
    {
        public KibanaConfiguration()
        {
            AzureBlobStorageConnectionString = ConnectionString.FromName("AzureBlobStorage.Kibana");
        }

        public KibanaConfiguration Change(Action<KibanaConfiguration> change)
        {
            if (change != null)
                change(this);

            return this;
        }

        public ConnectionString AzureBlobStorageConnectionString { get; set; }
    }
}