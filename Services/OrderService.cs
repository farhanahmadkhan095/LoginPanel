using Dapper;
using LoginPanel.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LoginPanel.Services
{
    public class OrderService : IOrderService
    {
        private readonly string _cs;

        public OrderService(IConfiguration cfg)
        {
            _cs = cfg.GetConnectionString("DefaultConnection")!;
        }

        public async Task<List<OrderListItem>> GetUserOrdersAsync(int userId)
        {
            using var con = new SqlConnection(_cs);

            var sql = @"
            SELECT 
                om.OrderId,
                om.OrderDate,
                om.TotalAmount,
                ISNULL(om.Status,'Placed') AS Status,

                (SELECT ISNULL(SUM(oim.Qty),0) 
                 FROM dbo.OrderItemMaster oim
                 WHERE oim.OrderId = om.OrderId) AS ItemCount,

                -- IMAGE URL LA RAHE HAIN
                (SELECT TOP 1 p.ImageUrl
                 FROM dbo.OrderItemMaster oim
                 JOIN dbo.Product p ON p.Id = oim.ProductId
                 WHERE oim.OrderId = om.OrderId
                 ORDER BY oim.OrderItemId) AS ImageUrl

            FROM dbo.OrderMaster om
            WHERE om.UserId = @UserId
            ORDER BY om.OrderId DESC;
            ";
            // ✅ THIS WAS MISSING


            var rows = await con.QueryAsync<OrderListItem>(sql, new { UserId = userId });
            return rows.ToList();
        }

        public async Task<OrderDetailsVM?> GetUserOrderDetailsAsync(int userId, int orderId)
        {
            using var con = new SqlConnection(_cs);

            var headerSql = @"
            SELECT 
                om.OrderId,
                om.OrderDate,
                om.TotalAmount,
                ISNULL(om.Status,'Placed') AS Status
            FROM dbo.OrderMaster om
            WHERE om.OrderId = @OrderId AND om.UserId = @UserId;";

            var header = await con.QueryFirstOrDefaultAsync<OrderDetailsVM>(
                headerSql, new { OrderId = orderId, UserId = userId });

            if (header == null) return null;

            var itemsSql = @"
            SELECT
                oim.ProductId,
                oim.ProductName,
                oim.Qty,
                oim.Price,
                oim.LineTotal,
                p.ImageUrl
            FROM dbo.OrderItemMaster oim
            JOIN dbo.Product p ON p.Id = oim.ProductId
            WHERE oim.OrderId = @OrderId
            ORDER BY oim.OrderItemId;
            ";

            var items = await con.QueryAsync<OrderDetailsItem>(itemsSql, new { OrderId = orderId });
            header.Items = items.ToList();

            return header;
        }
        public async Task<dynamic?> GetOrderSummaryAsync(int orderId, int userId)
        {
            using var con = new SqlConnection(_cs);

            var order = await con.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT OrderId, TotalAmount, Status, OrderDate
                FROM dbo.OrderMaster
                WHERE OrderId=@OrderId AND UserId=@UserId",
                new { OrderId = orderId, UserId = userId });

            return order;
        }

        public async Task SavePaymentAsync(int orderId, int userId, string paymentMode, string? txnRef)
        {
            using var con = new SqlConnection(_cs);

            var p = new DynamicParameters();
            p.Add("@OrderId", orderId);
            p.Add("@UserId", userId);
            p.Add("@PaymentMode", paymentMode);
            p.Add("@TxnRef", txnRef);

            await con.ExecuteAsync("dbo.usp_SavePayment", p, commandType: CommandType.StoredProcedure);
        }
        public async Task<dynamic?> GetOrderSummaryByOrderIdAsync(int orderId)
        {
            using var con = new SqlConnection(_cs);

            return await con.QueryFirstOrDefaultAsync<dynamic>(@"
        SELECT OrderId, TotalAmount, Status, OrderDate
        FROM dbo.OrderMaster
        WHERE OrderId = @OrderId",
                new { OrderId = orderId });
        }
        public interface IOrderService
        {
            Task<List<OrderListItem>> GetUserOrdersAsync(int userId);
            Task<OrderDetailsVM?> GetUserOrderDetailsAsync(int userId, int orderId);
            Task<dynamic?> GetOrderSummaryByOrderIdAsync(int orderId);
            Task SavePaymentAsync(int orderId, int userId, string paymentMode, string? txnRef);

            // ✅ ADD THIS
            Task SaveOrderAddressAsync(int orderId, CheckoutAddressVM addr);
        }
        public async Task SaveOrderAddressAsync(int orderId, CheckoutAddressVM addr)
        {
            using var con = new SqlConnection(_cs);

            var sql = @"
        INSERT INTO dbo.OrderAddress
        (
            OrderId,
            FullName,
            Mobile,
            AddressLine,
            City,
            Pincode,
            CreatedDate
        )
        VALUES
        (
            @OrderId,
            @FullName,
            @Mobile,
            @AddressLine,
            @City,
            @Pincode,
            GETDATE()
        );";

            await con.ExecuteAsync(sql, new
            {
                OrderId = orderId,
                addr.FullName,
                addr.Mobile,
                addr.AddressLine,
                addr.City,
                addr.Pincode
            });
        }

        public async Task<CheckoutAddressVM?> GetProfileAddressAsync(int userId)
        {
            using var con = new SqlConnection(_cs);

            return await con.QueryFirstOrDefaultAsync<CheckoutAddressVM>(@"
        SELECT TOP 1
            oa.FullName,
            oa.Mobile,
            oa.AddressLine,
            oa.City,
            oa.Pincode
        FROM dbo.OrderAddress oa
        JOIN dbo.OrderMaster om ON om.OrderId = oa.OrderId
        WHERE om.UserId = @UserId
        ORDER BY oa.CreatedDate DESC;
    ", new { UserId = userId });
        }
        public async Task CancelOrderAsync(int orderId, int userId)
        {
            using var con = new SqlConnection(_cs);

            var sql = @"
    UPDATE dbo.OrderMaster
    SET 
        Status = 'Cancelled',
        RefundStatus = 'Initiated',
        RefundAmount = TotalAmount
    WHERE OrderId = @OrderId AND UserId = @UserId
      AND Status IN ('Placed','Confirmed','Packed')";

            await con.ExecuteAsync(sql, new { OrderId = orderId, UserId = userId });
        }

    }
}