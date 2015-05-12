using System;
using System.Configuration;
using System.Linq;
using FluentMigrator;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Factories;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Experiments.Migrations
{
    [Migration(1427839041)]
    public class M1427839041_SetupMonitorConfiguration : Migration
    {
        public override void Up()
        {
            IConfigurationService service = ObjectFactory.Instance.Resolve<IConfigurationService>();

            MonitorConfiguration configuration = service.Get<MonitorConfiguration>();

            configuration.Targets.Single(x => x.Equals(Target.Service)).Recipients = new[] { "bhk@vertica.dk" };
            configuration.SubjectPrefix = String.Format("{0} Integration Service", ConfigurationManager.AppSettings["Environment"]);

            service.Save(configuration, "Migration", createArchiveBackup: true);
        }

        public override void Down()
        {
        }
    }
}