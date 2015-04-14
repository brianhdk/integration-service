using System.Configuration;
using FluentMigrator;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Factories;

namespace Vertica.Integration.Experiments.Extend_IntegrationDb
{
    [Migration(1427839050)]
    public class M1427839050_ChangeSubjectPrefixOnMonitorConfiguration : Migration
    {
        public override void Up()
        {
            IConfigurationService service = 
                ObjectFactory.Instance.Resolve<IConfigurationService>();

            MonitorConfiguration configuration = service.Get<MonitorConfiguration>();
            configuration.SubjectPrefix = ConfigurationManager.AppSettings["Environment"] + " Integration Service";

            service.Save(configuration, "Migration", createArchiveBackup: true);
        }

        public override void Down()
        {
        }
    }
}