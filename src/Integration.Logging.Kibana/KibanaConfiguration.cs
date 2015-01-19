namespace Vertica.Integration.Logging.Kibana
{
    public class KibanaConfiguration
    {
        public KibanaConfiguration()
        {
            AzureBlobStorageConnectionStringName = "AzureBlobStorage";
        }

        public string AzureBlobStorageConnectionStringName { get; set; }
    }
}