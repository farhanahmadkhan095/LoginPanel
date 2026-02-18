namespace LoginPanel.Models
{
    public class InventoryHistoryVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = ""; // nullable warning avoid

        public int OldQty { get; set; }
        public int QtyChange { get; set; }
        public int NewQty { get; set; }

        public string Reason { get; set; } = "";
        public string ActionType { get; set; } = ""; // Sale / Purchase / Correction
        public DateTime ActionDate { get; set; }
    }
}
