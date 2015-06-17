using System;
using System.Linq;
using FluentMigrator;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Experiments.Migrations.IntegrationDb
{
    [Migration(1427839041)]
    public class M1427839041_SetupMonitorConfiguration : IntegrationMigration
    {
        public override void Up()
        {
            MonitorConfiguration configuration = GetConfiguration<MonitorConfiguration>();

            configuration.Targets.Single(x => x.Equals(Target.Service)).Recipients = new[] { "bhk@vertica.dk" };
            configuration.SubjectPrefix = String.Format("{0} Integration Service", AppSettings["Environment"]);

            SaveConfiguration(configuration);
        }

        public override void Down()
        {
        }
    }
}