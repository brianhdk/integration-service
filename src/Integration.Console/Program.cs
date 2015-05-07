using Vertica.Integration.Azure;
using Vertica.Integration.Experiments;
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
                .UseAzure(azure => azure
                    .ReplaceArchiveWithBlobStorage(ConnectionString.FromName("AzureBlobStorage.Archive"))
                    .ReplaceFileSystemWithBlobStorage(ConnectionString.FromName("AzureBlobStorage.FileSystem")))
                .Dapper(dapper => dapper.AddConnection(new CustomDb()))
                .Migration(migration => migration
                    .IncludeFromNamespaceOfThis<M1427839039_NewTable>(DatabaseServer.SqlServer2014, builder.DatabaseConnectionString))
                .Tasks(tasks => tasks.ScanFromAssemblyOfThis<ArchiveTesterTask>())))
			{
			    context.Execute(args);
			}
		}
	}
}