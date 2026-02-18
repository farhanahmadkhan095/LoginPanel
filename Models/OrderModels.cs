namespace LoginPanel.Models
{
    public class OrderListItem
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public int ItemCount { get; set; }
        public string ProductName { get; set; } = "";
        public string ImageUrl { get; set; }



    }

    public class OrderDetailsVM
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public string? RefundStatus { get; set; }

        public List<OrderDetailsItem> Items { get; set; } = new();
    }

    public class OrderDetailsItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal { get; set; }
        public string ImageUrl { get; set; }


    }
}

