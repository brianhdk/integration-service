using System;
using System.Runtime.InteropServices;

namespace Vertica.Integration.UCommerce.Maintenance
{
    [Guid("7BC9E2B5-E201-45DB-8B82-F7EB24CCB07A")]
    public class UCommerceMaintenanceConfiguration
    {
        public UCommerceMaintenanceConfiguration()
        {
            IndexMaintenance = new IndexMaintenanceConfiguration();
        }

        public IndexMaintenanceConfiguration IndexMaintenance { get; }

        public class IndexMaintenanceConfiguration
        {
            public IndexMaintenanceConfiguration()
            {
                Enabled = true;

                Tables = new[]
                {
                    new TableConfiguration("uCommerce_PurchaseOrder")
                    {
                        Indicies = new []
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_BasketId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_BillingAddressId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_CurrencyId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_CustomerId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_OrderGuid"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_OrderNumber"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_OrderStatusId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_ProductCatalogGroupId"),
                            new TableConfiguration.IndiciesConfiguration("uCommerce_PK_Order")
                        }
                    }
                };
            }

            public TableConfiguration[] Tables { get; set; }

            public bool Enabled { get; set; }

            public class TableConfiguration
            {
                public TableConfiguration(string name)
                {
                    if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty", nameof(name));
                    
                    Name = name;
                    Enabled = true;
                }

                public string Name { set; get; }
                public IndiciesConfiguration[] Indicies { get; set; }

                public bool Enabled { get; set; }
                
                public class IndiciesConfiguration
                {
                    public IndiciesConfiguration(string name)
                    {
                        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty", nameof(name));
                        
                        Name = name;
                        Reorganize = true;
                    }

                    public string Name { get; set; }
                    public bool Reorganize { get; set; }
                }
            }
        }
    }
}