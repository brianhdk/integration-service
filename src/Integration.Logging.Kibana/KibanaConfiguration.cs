using System;

namespace Vertica.Integration.Logging.Kibana
{
    public class KibanaConfiguration
    {
        public KibanaConfiguration()
        {
            AzureBlobStorageConnectionStringName = "AzureBlobStorage.Kibana";
        }

        public KibanaConfiguration Change(Action<KibanaConfiguration> change)
        {
            if (change != null)
                change(this);

            return this;
        }

        public string AzureBlobStorageConnectionStringName { get; set; }
    }
}