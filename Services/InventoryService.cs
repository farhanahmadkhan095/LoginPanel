using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LoginPanel.Models;

namespace LoginPanel.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly string _connectionString;

        public InventoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        private IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);

        // 🔹 Inventory List (Current only)
        public async Task<IEnumerable<InventoryDto>> GetAllAsync()
        {
            using var con = CreateConnection();
            return await con.QueryAsync<InventoryDto>(
                "usp_Inventory_GetAll",
                commandType: CommandType.StoredProcedure
            );
        }

        // 🔹 Product Dropdown
        public async Task<IEnumerable<SelectListItem>> GetProductsAsync()
        {
            using var con = CreateConnection();
            var list = await con.QueryAsync(
                "SELECT Id AS Value, Name AS Text FROM Product ORDER BY Name"
            );

            return list.Select(x => new SelectListItem
            {
                Value = x.Value.ToString(),
                Text = x.Text
            }).ToList();
        }

        // 🔹 Reason Dropdown
        public async Task<IEnumerable<SelectListItem>> GetReasonsAsync()
        {
            using var con = CreateConnection();
            var list = await con.QueryAsync(
                @"SELECT Id AS Value, Reason AS Text
                  FROM InventoryAdjustmentReason
                  WHERE IsDeleted = 0"
            );

            return list.Select(x => new SelectListItem
            {
                Value = x.Value.ToString(),
                Text = x.Text
            }).ToList();
        }

        // 🔹 Inventory Detail by Product (CURRENT QTY ONLY)
        public async Task<InventoryAdjustmentDto> GetInventoryByProductAsync(int productId)
        {
            using var con = CreateConnection();

            return await con.QuerySingleOrDefaultAsync<InventoryAdjustmentDto>(
                @"SELECT 
                      i.ProductId,
                      p.Name AS ProductName,
                      i.Quantity AS CurrentQty,
                      i.UnitCost,
                      i.TotalCost
                  FROM Inventory i
                  JOIN Product p ON p.Id = i.ProductId
                  WHERE i.ProductId = @ProductId
                    AND i.IsCurrent = 1",
                new { ProductId = productId }
            );
        }

        // 🔹 SALE / PURCHASE ADJUSTMENT (PLUS / MINUS)
        public async Task AdjustInventoryAsync(int productId, int qtyChange, int reasonId)
        {
            using var con = CreateConnection();

            var unitCost = await con.QuerySingleAsync<decimal>(
                "SELECT Price FROM Product WHERE Id = @Id",
                new { Id = productId }
            );

            await con.ExecuteAsync(
                "usp_ManageInventory",
                new
                {
                    ProductId = productId,
                    QuantityChange = qtyChange,
                    UnitCost = unitCost,
                    ReasonId = reasonId
                },
                commandType: CommandType.StoredProcedure
            );
        }

        // 🔹 INVENTORY CORRECTION (BASE QTY REPLACE – NO PLUS)
        public async Task CorrectInventoryAsync(int productId, int newQuantity, int reasonId, string correctionReason)
        {
            using var con = CreateConnection();
            await con.ExecuteAsync(
                "sp_CorrectInventory",
                new
                {
                    ProductId = productId,
                    NewQuantity = newQuantity,
                    ReasonId = reasonId,
                    CorrectionReason = correctionReason
                },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<int> GetCurrentQtyAsync(int productId)
        {
            using var con = CreateConnection();

            return await con.ExecuteScalarAsync<int>(
                @"SELECT Quantity
                  FROM Inventory
                  WHERE ProductId = @ProductId
                    AND IsCurrent = 1",
                new { ProductId = productId }
            );
        }

        // 🔹 RECENT ADJUSTMENT HISTORY (BASE NOT SHOWN)
        public async Task<IEnumerable<InventoryAdjustmentHistoryDto>> GetRecentAdjustmentsAsync(int productId)
        {
            using var con = CreateConnection();
            return await con.QueryAsync<InventoryAdjustmentHistoryDto>(
                "usp_InventoryAdjustment_GetRecent",
                new { ProductId = productId },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<IEnumerable<InventoryHistoryVM>> GetInventoryHistoryAsync(int productId)
        {
            using var con = CreateConnection();

            var sql = @"
                          SELECT
                        (H.OldQty - H.QtyChange) AS OldQty,
                        H.QtyChange AS QtyChange,
                        H.NewQty AS NewQty,
                        R.Reason,
                        'Adjustment' AS ActionType,
                        H.AdjustmentDate AS ActionDate
                    FROM InventoryAdjustmentHistory H
                    JOIN InventoryAdjustmentReason R ON R.Id = H.ReasonId
                    WHERE H.ProductId = @productId

                    UNION ALL

   
                    SELECT
                        (C.Quantity - C.QuantityChange) AS OldQty,
                        C.QuantityChange AS QtyChange,
                        C.Quantity AS NewQty,
                        C.CorrectionReason AS Reason,
                        'Correction' AS ActionType,
                        C.EntryDate AS ActionDate
                    FROM InventoryCorrectionLog C
                    WHERE C.ProductId = @productId

                    ORDER BY ActionDate DESC
                    ";

            return await con.QueryAsync<InventoryHistoryVM>(
                sql,
                new { ProductId = productId }
            );
        }
    }
}

