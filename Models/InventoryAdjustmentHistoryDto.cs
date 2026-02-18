namespace LoginPanel.Models
{
    public class InventoryAdjustmentHistoryDto
    {
        public DateTime AdjustmentDate { get; set; }
        public int QtyChange { get; set; }
        public string ReasonName { get; set; }
        public int CurrentQtyAfter { get; set; }
    }
}