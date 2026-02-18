using LoginPanel.Enums;

namespace LoginPanel.Models
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = "COD"; // default
        public string? TransactionId { get; set; }       // UPI/Card reference

    }
}
