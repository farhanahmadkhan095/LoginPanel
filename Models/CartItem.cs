namespace LoginPanel.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal ProductPrice { get; set; }

        public int Qty { get; set; }
        public int AvailableQty { get; set; }
        public string ImageUrl { get; set; } = "/images/no-image.jpg";
    }
}
