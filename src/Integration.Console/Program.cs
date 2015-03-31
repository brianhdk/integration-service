using Vertica.Integration.Azure;
using Vertica.Integration.Experiments.Azure;
using Vertica.Integration.Experiments.Custom_Database;
using Vertica.Integration.Experiments.Extend_IntegrationDb;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Portal;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ApplicationContext context = ApplicationContext.Create(builder => builder
                .UsePortal()
                .UseAzure(azure => azure.ReplaceArchiverWithBlobStorage(ConnectionString.FromName("AzureBlobStorage.Archiver")))
                .Dapper(dapper => dapper.AddConnection(new CustomDb()))
                .Migration(migration => 
                    migration.IncludeFromNamespaceOfThis<M1427839039_NewTable>(DatabaseServer.SqlServer2014, builder.DatabaseConnectionString))
                .Tasks(tasks => tasks.ScanFromAssemblyOfThis<AzureArchiverTesterTask>())))
			{
			    context.Execute(args);
			}
		}
	}
}