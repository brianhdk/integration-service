using FluentMigrator;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Factories;

namespace Vertica.Integration.Experiments.Migrations
{
    [Migration(1431459508)]
    public class M1431459508_MergeMaintenanceConfiguration : Migration
    {
        public override void Up()
        {
            IConfigurationService service = ObjectFactory.Instance.Resolve<IConfigurationService>();

            Configuration oldConfiguration =
                service.Get("Vertica.Integration.Domain.Core.MaintenanceConfiguration, Vertica.Integration");

            if (oldConfiguration != null)
            {
                // Ensure new MaintenanceConfiguration
                service.Get<MaintenanceConfiguration>();

                Configuration newConfiguration = service.Get("FBF783F5-0210-448D-BEB9-FD0E9AD6CABF");
                newConfiguration.JsonData = oldConfiguration.JsonData;

                service.Save(newConfiguration, "Migration", createArchiveBackup: true);
                service.Delete(oldConfiguration.Id);
            }
        }

        public override void Down()
        {
        }
    }
}