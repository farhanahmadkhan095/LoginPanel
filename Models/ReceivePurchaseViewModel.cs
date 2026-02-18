using System;
using System.Collections.Generic;

namespace LoginPanel.Models
{
    public class ReceivePurchaseViewModel
    {
        public int PurchaseOrderId { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; } = "";
        public DateTime OrderDate { get; set; }

        public int UserId { get; set; }

        public List<ReceiveItemModel> Items { get; set; } = new();
    }

    public class ReceiveItemModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";

        public int OrderedQty { get; set; }
        public int ReceivedQty { get; set; }
    }
}
