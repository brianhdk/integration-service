using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;
using Vertica.Integration.UCommerce.Database;
using Vertica.Utilities.Extensions.EnumerableExt;

namespace Vertica.Integration.UCommerce.Maintenance
{
    public class UCommerceIndexMaintenanceStep : Step<MaintenanceWorkItem>
    {
        private readonly IDbFactory<UCommerceDb> _uCommerceDb;
        private readonly IConfigurationService _configuration;

        public UCommerceIndexMaintenanceStep(IDbFactory<UCommerceDb> uCommerceDb, IConfigurationService configuration)
        {
            _uCommerceDb = uCommerceDb;
            _configuration = configuration;
        }

        public override Execution ContinueWith(ITaskExecutionContext<MaintenanceWorkItem> context)
        {
            UCommerceMaintenanceConfiguration configuration = context.EnsureConfiguration(_configuration);

            if (!configuration.IndexMaintenance.Enabled)
                return Execution.StepOver;

            return base.ContinueWith(context);
        }

        public override void Execute(ITaskExecutionContext<MaintenanceWorkItem> context)
        {
            UCommerceMaintenanceConfiguration configuration = context.EnsureConfiguration(_configuration);

            string script = BuildScript(configuration);

            if (script.Length > 0)
            {
                context.Log.Message(@"Executing sql: {0}", script);

                using (IDbConnection connection = _uCommerceDb.GetConnection())
                {
                    connection.Open();

                    var command = new CommandDefinition(
                        script,
                        cancellationToken: context.CancellationToken,
                        commandTimeout: configuration.IndexMaintenance.CommandTimeout.GetValueOrDefault(int.MaxValue));

                    connection.ExecuteAsync(command).Wait(context.CancellationToken);
                }
            }
        }

        private static string BuildScript(UCommerceMaintenanceConfiguration configuration)
        {
            var script = new StringBuilder();

            foreach (var table in configuration.IndexMaintenance.Tables.EmptyIfNull())
            {
                if (!table.Enabled)
                    continue;

                script.Append(string.Join(string.Empty, table.Indicies.Where(x => x.Reorganize).Select(index => $@"
ALTER INDEX [{index.Name}] ON [dbo].[{table.Name}] REORGANIZE;")));
            }

            return script.ToString();
        }

        public override string Description => "Performs index maintenance on configurable uCommerce tables.";
    }
}