using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;
using Vertica.Integration.UCommerce.Database;

namespace Vertica.Integration.UCommerce
{
    public class UCommerceIndexMaintenanceStep : Step<MaintenanceWorkItem>
    {
        private readonly IDbFactory<UCommerceDb> _uCommerceDb;

        public UCommerceIndexMaintenanceStep(IDbFactory<UCommerceDb> uCommerceDb)
        {
            _uCommerceDb = uCommerceDb;
        }

        public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
            using (IDbSession session = _uCommerceDb.OpenSession())
            {
                session.Execute(@"
ALTER INDEX [uCommerce_PK_Order] ON [dbo].[uCommerce_PurchaseOrder] REORGANIZE
ALTER INDEX [IX_Order] ON [dbo].[uCommerce_PurchaseOrder] REORGANIZE
ALTER INDEX [IX_uCommerce_PurchaseOrder_BasketId] ON [dbo].[uCommerce_PurchaseOrder] REORGANIZE
");
            }
        }

        public override string Description
        {
            get { return "Performs index maintenance on certain uCommerce tables."; }
        }
    }
}