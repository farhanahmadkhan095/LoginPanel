namespace LoginPanel.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal ProductPrice { get; set; }

        public string ProductDescription { get; set; }

        public bool IsActive { get; set; }
        public int AvailableQty { get; set; }
        public string CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int EntryBy { get; set; }
        public bool IsLowestPrice { get; set; }
        public bool IsTrending { get; set; }

    }
}