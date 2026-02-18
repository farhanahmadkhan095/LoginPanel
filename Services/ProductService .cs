using Dapper;
using LoginPanel.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LoginPanel.Services
{
    public class ProductService : IProductService
    {
        // ✅ SINGLE DB ACCESS PATTERN
        private readonly IDbConnection _db;

        public ProductService(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<ProductModel>> GetProductsAsync()
        {
            var sql = @"
                SELECT
                    Id          AS ProductId,
                    Name        AS ProductName,
                    Price       AS ProductPrice,
                    Description AS ProductDescription,
                    IsActive    AS IsActive,
                    CreatedDate AS CreatedDate,
                    ModifiedDate AS ModifiedDate,
                    EntryBy     AS EntryBy
                FROM dbo.Product
                WHERE IsActive = 1
                ORDER BY Id DESC;";

            return (await _db.QueryAsync<ProductModel>(sql)).AsList();
        }

        public async Task<ProductModel?> GetProductAsync(int id)
        {
            var sql = @"
                SELECT TOP 1
                    p.Id            AS ProductId,
                    p.Name          AS ProductName,
                    p.Price         AS ProductPrice,
                    p.Description   AS ProductDescription,
                    ISNULL(p.IsActive, 1) AS IsActive,
                    p.CreatedDate   AS CreatedDate,
                    p.ModifiedDate  AS ModifiedDate,
                    p.EntryBy       AS EntryBy,
                    ISNULL(inv.Quantity, 0) AS AvailableQty
                FROM dbo.Product p
                OUTER APPLY
                (
                    SELECT TOP 1 Quantity
                    FROM dbo.Inventory
                    WHERE ProductId = p.Id AND IsCurrent = 1
                    ORDER BY InventoryId DESC
                ) inv
                WHERE p.Id = @Id;";

            return await _db.QueryFirstOrDefaultAsync<ProductModel>(sql, new { Id = id });
        }

        public async Task<int> GetStockAsync(int productId)
        {
            var sql = @"
                SELECT TOP 1 Quantity
                FROM dbo.Inventory
                WHERE ProductId = @ProductId AND IsCurrent = 1
                ORDER BY InventoryId DESC;";

            return await _db.ExecuteScalarAsync<int?>(sql, new { ProductId = productId }) ?? 0;
        }

        // 🔥 INVENTORY DEDUCT — PAYMENT SUCCESS KE BAAD HI
        public async Task DeductInventoryAfterPaymentAsync(int orderId)
        {
            var sql = @"
                UPDATE inv
                SET
                    inv.Quantity = inv.Quantity - oim.Qty,
                    inv.QuantityChange = inv.QuantityChange - oim.Qty,
                    inv.TotalCost = (inv.Quantity - oim.Qty) * inv.UnitCost,
                    inv.ModifiedDate = GETDATE()
                FROM dbo.Inventory inv
                JOIN dbo.OrderItemMaster oim 
                    ON oim.ProductId = inv.ProductId
                WHERE oim.OrderId = @OrderId
                  AND inv.IsCurrent = 1;
            ";

            await _db.ExecuteAsync(sql, new { OrderId = orderId });
        }
    }
}
