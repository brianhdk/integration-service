using System;
using Castle.MicroKernel;
using FluentMigrator.VersionTableInfo;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [VersionTableMetaData]
    public class VersionTable : DefaultVersionTableMetaData
    {
        public override string TableName => Configuration.TableName(IntegrationDbTable.BuiltIn_VersionInfo);

        private IIntegrationDatabaseConfiguration Configuration => Kernel.Resolve<IIntegrationDatabaseConfiguration>();
        
        private IKernel Kernel
        {
            get
            {
                var kernel = ApplicationContext as IKernel;

                if (kernel == null)
                    throw new InvalidOperationException($"Unable to cast {nameof(ApplicationContext)} to {nameof(IKernel)}. ApplicationContext: {ApplicationContext?.ToString() ?? "<null>"}");

                return kernel;
            }
        }
    }
}