namespace LoginPanel.Models
{
    public class InventoryDto
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int QuantityChange { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public string ReasonName { get; set; }
		public bool IsPositive { get; set; }
		public DateTime EntryDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsCurrent { get; set; }

        public string QuantityChangeWithSign
        {
            get
            {
                if (QuantityChange > 0) return "+" + QuantityChange;
                else if (QuantityChange < 0) return QuantityChange.ToString();
                else return "0";
            }
        }
    }
}
