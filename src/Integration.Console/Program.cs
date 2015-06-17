using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Experiments;
using Vertica.Integration.Experiments.Migrations;
using Vertica.Integration.Experiments.Migrations.CustomDb;
using Vertica.Integration.Experiments.Migrations.IntegrationDb;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Logging.Elmah;
using Vertica.Integration.Portal;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ApplicationContext context = ApplicationContext.Create(builder => builder
                .UsePortal()))
			{
			    context.Execute(args);
			}
		}
	}
}