using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using LoginPanel.Models;
using System.Collections.Generic;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly string _connectionString;

    public PurchaseOrderService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // Get vendors
    public PurchaseOrderViewModel GetCreatePageData()
    {
        using var con = new SqlConnection(_connectionString);
        var vendors = con.Query<VendorModel>(
            "SELECT VendorId, VendorName FROM VendorMaster WHERE IsActive = 1"
        ).AsList();

        return new PurchaseOrderViewModel
        {
            Vendors = vendors
        };
    }

    public List<PurchaseOrderItemModel> GetVendorProductsWithPrice(int vendorId)
    {
        using var con = new SqlConnection(_connectionString);
        string sql = @"
            SELECT p.Id AS ProductId, p.Name AS ProductName, vpp.Price, 0 AS Quantity
            FROM VendorProductPricing vpp
            INNER JOIN Product p ON p.Id = vpp.ProductId
            WHERE vpp.VendorId = @VendorId";

        return con.Query<PurchaseOrderItemModel>(sql, new { VendorId = vendorId }).AsList();
    }

   
    public bool CreatePurchaseOrder(PurchaseOrderViewModel model)
    {
        try
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();

            var poId = con.ExecuteScalar<int>(
                @"INSERT INTO PurchaseOrder (VendorId, OrderDate, TotalAmount, Status)
                    VALUES (@VendorId, GETDATE(), @TotalAmount, @Status);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new
                {
                    VendorId = model.SelectedVendorId,
                    model.TotalAmount,
                Status =  model.OrderStatus
                }
            );

            // Insert each item
            foreach (var item in model.Items)
            {
                con.Execute(
                    @"INSERT INTO PurchaseOrderItem
                      (PurchaseOrderId, ProductId, Quantity, Price)
                      VALUES (@PurchaseOrderId, @ProductId, @Quantity, @Price)",
                    new
                    {
                        PurchaseOrderId = poId,
                        item.ProductId,
                        item.Quantity,
                        item.Price
                      
                    }
                );
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
