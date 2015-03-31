using FluentMigrator;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Factories;

namespace Vertica.Integration.Experiments.Extend_IntegrationDb
{
    [Migration(1427839041)]
    public class M1427839041_AddRecipientToMonitorConfiguration : Migration
    {
        public override void Up()
        {
            IConfigurationProvider provider = 
                ObjectFactory.Instance.Resolve<IConfigurationProvider>();

            MonitorConfiguration configuration = provider.Get<MonitorConfiguration>();
            configuration.Targets[0].Recipients = new[] { "bhk@vertica.dk" };

            provider.Save(configuration, "Migration", createArchiveBackup: true);
        }

        public override void Down()
        {
        }
    }
}