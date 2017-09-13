using Nest;
using Vertica.Integration;
using Vertica.Integration.Elasticsearch;
using Vertica.Integration.Elasticsearch.Infrastructure.Clusters;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;

namespace Experiments.Console.Elasticsearch
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .UseElasticsearch(elasticsearch => elasticsearch
                    .Clusters(clusters => clusters
                        .DefaultConnection(ConnectionString.FromText("http://localhost:9200/"))
                        .AddConnection(new AnotherElasticCluster(ConnectionString.FromText("http://localhost:9201/")))))))
            {
                var writer = context.Resolve<IConsoleWriter>();

                // Ping cluster A
                IElasticClient defaultCluster = context.Resolve<IElasticClientFactory>().Get();
                writer.WriteLine(defaultCluster.Ping().DebugInformation);

                writer.WriteLine();
                writer.WriteLine("------------------");
                writer.WriteLine();

                // Ping cluster B
                IElasticClient anotherCluster = context.Resolve<IElasticClientFactory<AnotherElasticCluster>>().Get();
                writer.WriteLine(anotherCluster.Ping().DebugInformation);
            }
        }

        public class AnotherElasticCluster : Connection
        {
            public AnotherElasticCluster(ConnectionString connectionString)
                : base(connectionString)
            {
            }
        }
    }
}