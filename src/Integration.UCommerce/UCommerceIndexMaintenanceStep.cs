using System;
using System.Linq;
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
            // TODO: Configurable - allow external configuration of indexes
            // Parse index names - only allow chars/digits/_-

            string[] indexNames =
            {
                "IX_uCommerce_PurchaseOrder_BasketId",
                "IX_uCommerce_PurchaseOrder_BillingAddressId",
                "IX_uCommerce_PurchaseOrder_CurrencyId",
                "IX_uCommerce_PurchaseOrder_CustomerId",
                "IX_uCommerce_PurchaseOrder_OrderGuid",
                "IX_uCommerce_PurchaseOrder_OrderNumber",
                "IX_uCommerce_PurchaseOrder_OrderStatusId",
                "IX_uCommerce_PurchaseOrder_ProductCatalogGroupId",
                "uCommerce_PK_Order"
            };

            using (IDbSession session = _uCommerceDb.OpenSession())
            {
                session.Execute(string.Join(Environment.NewLine, indexNames.Select(indexName => $@"
ALTER INDEX [{indexName}] ON [dbo].[uCommerce_PurchaseOrder] REORGANIZE;")));
            }
        }

        public override string Description => "Performs index maintenance on certain uCommerce tables.";
    }
}