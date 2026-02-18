using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using LoginPanel.Models;
using LoginPanel.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;

namespace LoginPanel.Services
{
    public class ReceivedPurchaseService : IReceivedPurchaseService
    {
        private readonly string _connectionString;

        public ReceivedPurchaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<PurchaseOrderViewModel> GetPendingPurchaseOrders()
        {
            using var con = new SqlConnection(_connectionString);

            return con.Query<PurchaseOrderViewModel>(
                @"SELECT PurchaseOrderId, OrderDate, Status AS OrderStatus
                  FROM PurchaseOrder
                  WHERE Status = 0"
            ).AsList();
        }

        public ReceivePurchaseViewModel GetReceivePageData(int poId)
        {
            using var con = new SqlConnection(_connectionString);



            
            var header = con.QuerySingle<ReceivePurchaseViewModel>(
                @"SELECT po.PurchaseOrderId, v.VendorName, po.OrderDate
                  FROM PurchaseOrder po
                  JOIN VendorMaster v ON v.VendorId = po.VendorId
                  WHERE po.PurchaseOrderId = @Id",
                new { Id = poId });

            var items = con.Query<ReceiveItemModel>(
                @"SELECT p.Id AS ProductId,
                         p.Name AS ProductName,
                         poi.Quantity AS OrderedQty,
                         0 AS ReceivedQty
                  FROM PurchaseOrderItem poi
                  JOIN Product p ON p.Id = poi.ProductId
                  WHERE poi.PurchaseOrderId = @Id",
                new { Id = poId }).AsList();

            header.Items = items;
            return header;
        }

        public void SaveReceivedItems(ReceivePurchaseViewModel model, int userId)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var tran = con.BeginTransaction();

            try
            {
                foreach (var item in model.Items)
                {
                    if (item.ReceivedQty <= 0)
                        continue;

                    // 1) Insert receive record
                    con.Execute(
                        @"INSERT INTO ReceivedPurchaseOrder
                          (PurchaseOrderId, ProductId, ReceivedQty)
                          VALUES (@POId, @ProductId, @Qty)",
                        new
                        {
                            POId = model.PurchaseOrderId,
                            ProductId = item.ProductId,
                            Qty = item.ReceivedQty
                        },
                        tran
                    );

                    // 2) UnitCost
                    decimal unitCost = con.QuerySingle<decimal>(
                        @"SELECT Price FROM Product WHERE Id = @ProductId",
                        new { ProductId = item.ProductId },
                        tran
                    );

                    // 3) Update Inventory (✅ pass UserId)
                    con.Execute(
                        "dbo.usp_ManageInventory",
                        new
                        {
                            ProductId = item.ProductId,
                            QuantityChange = item.ReceivedQty,
                            UnitCost = unitCost,
                            ReasonId = 1,
                            UserId = userId
                        },
                        tran,
                        commandType: CommandType.StoredProcedure
                    );
                }

                // 4) Mark PO received
                con.Execute(
                    @"UPDATE PurchaseOrder
                      SET Status = 1
                      WHERE PurchaseOrderId = @Id",
                    new { Id = model.PurchaseOrderId },
                    tran
                );

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        public bool IsPurchaseOrderReceived(int purchaseOrderId)
        {
            using var con = new SqlConnection(_connectionString);

            return con.ExecuteScalar<int>(
                @"SELECT COUNT(1)
                  FROM ReceivedPurchaseOrder
                  WHERE PurchaseOrderId = @Id",
                new { Id = purchaseOrderId }
            ) > 0;
        }
        public List<PurchaseOrderViewModel> GetAllPurchaseOrders()
        {
            using var con = new SqlConnection(_connectionString);

            return con.Query<PurchaseOrderViewModel>(
                @"SELECT 
            PurchaseOrderId,
            OrderDate,
            Status AS OrderStatus
          FROM PurchaseOrder"
            ).AsList();
        }

    }
}
