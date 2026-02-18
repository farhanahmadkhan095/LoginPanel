using LoginPanel.Models;

namespace LoginPanel.Services
{
    public interface IOrderService
    {
        Task<List<OrderListItem>> GetUserOrdersAsync(int userId);

        Task<OrderDetailsVM?> GetUserOrderDetailsAsync(int userId, int orderId);

        // Existing (used for Orders / COD / security)
        Task<dynamic?> GetOrderSummaryAsync(int orderId, int userId);

        // ✅ ADD THIS (used for Razorpay payment initiation)
        Task<dynamic?> GetOrderSummaryByOrderIdAsync(int orderId);

        Task SavePaymentAsync(int orderId, int userId, string paymentMode, string? txnRef);
        Task SaveOrderAddressAsync(int orderId, CheckoutAddressVM addr);
        Task<CheckoutAddressVM?> GetProfileAddressAsync(int userId);
        Task CancelOrderAsync(int orderId, int userId);

    }
}
