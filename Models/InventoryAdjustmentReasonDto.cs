namespace LoginPanel.Models
{
    public class InventoryAdjustmentReasonDto
    {
        public int Id { get; set; }
        public string Reason { get; set; }
		public bool IsPositive { get; set; }
		public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

}
