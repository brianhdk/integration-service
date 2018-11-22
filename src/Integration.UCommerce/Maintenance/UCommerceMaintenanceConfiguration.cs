using System;
using System.Linq;
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
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_CompletedDate"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_PurchaseOrder_CreatedDate"),
                            new TableConfiguration.IndiciesConfiguration("uCommerce_PK_Order")
                        }
                    },
                    new TableConfiguration("uCommerce_OrderProperty")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_OrderProperty_OrderId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_OrderProperty_OrderLineId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_OrderProperty_Key"),
                            new TableConfiguration.IndiciesConfiguration("PK_uCommerce_OrderProperty")
                        }
                    },
                    new TableConfiguration("uCommerce_OrderLine")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_OrderLine_OrderId")
                        }
                    },
                    new TableConfiguration("uCommerce_OrderStatusAudit")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_OrderStatusAudit_OrderId")
                        }
                    },
                    new TableConfiguration("uCommerce_OrderAddress")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_OrderAddress_OrderId")
                        }
                    },
                    new TableConfiguration("uCommerce_Customer")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Customer_MemberId")
                        }
                    },
                    new TableConfiguration("uCommerce_Product")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("UX_uCommerce_Product_Sku_VariantSku"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Product_Sku"),
                            new TableConfiguration.IndiciesConfiguration("uCommerce_PK_Product")
                        }
                    },
                    new TableConfiguration("uCommerce_ProductDescription")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_ProductDescription_CultureCode"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_ProductDescription_ProductId"),
                            new TableConfiguration.IndiciesConfiguration("UX_uCommerce_ProductDescription_ProductId_CultureCode"),
                            new TableConfiguration.IndiciesConfiguration("uCommerce_PK_ProductDescription")
                        }
                    },
                    new TableConfiguration("uCommerce_Category")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Category_Name"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Category_Guid"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Category_DisplayOnSite"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Category_Deleted"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Category_DefinitionId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Category_ParentCategoryId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Category_ProductCatalogId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_Category_SortOrder"),
                            new TableConfiguration.IndiciesConfiguration("uCommerce_PK_Category")
                        }
                    },
                    new TableConfiguration("uCommerce_CategoryProductRelation")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_CategoryProductRelation_ProductId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_CategoryProductRelation_CategoryId"),
                            new TableConfiguration.IndiciesConfiguration("UX_uCommerce_CategoryProductRelation_CategoryId_ProductId")
                        }
                    },
                    new TableConfiguration("uCommerce_CategoryDescription")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_CategoryDescription_CultureCode"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_CategoryDescription_CategoryId"),
                            new TableConfiguration.IndiciesConfiguration("UX_uCommerce_CategoryDescription_CultureCode_CategoryId"),
                            new TableConfiguration.IndiciesConfiguration("UX_uCommerce_CategoryDescription_CultureCode_CategoryId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_CategoryDescription_RenderAsContent ")
                            
                        }
                    },
                    new TableConfiguration("uCommerce_CategoryProperty")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_CategoryProperty_CultureCode"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_CategoryProperty_CategoryId"),
                            new TableConfiguration.IndiciesConfiguration("UX_uCommerce_CategoryProperty_CategoryId_CultureCode_DefinitionFieldId"),
                            new TableConfiguration.IndiciesConfiguration("IX_uCommerce_CategoryProperty_DefinitionFieldId"),
                            new TableConfiguration.IndiciesConfiguration("PK_uCommerce_CategoryProperty")
                        }
                    },
                    new TableConfiguration("uCommerce_VersionedField")
                    {
                        Indicies = new[]
                        {
                            new TableConfiguration.IndiciesConfiguration("PK_uCommerce_VersionedField ")
                        }
                    }
                }
                .OrderBy(x => x.Name)
                .ToArray();
            }

            public TableConfiguration[] Tables { get; set; }
            public int? CommandTimeout { get; set; }

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