using Vertica.Integration.Azure;
using Vertica.Integration.Experiments.Azure;
using Vertica.Integration.Experiments.Custom_Database;
using Vertica.Integration.Infrastructure;
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
                .Tasks(tasks => tasks.ScanFromAssemblyOfThis<AzureArchiverTesterTask>())))
			{
			    context.Execute(args);
			}
		}
	}
}