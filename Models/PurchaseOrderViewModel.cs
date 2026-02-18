using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LoginPanel.Models
{
    public class PurchaseOrderViewModel
    {
        public int PurchaseOrderId { get; set; }
        public int SelectedVendorId { get; set; }
        public DateTime OrderDate { get; set; }

        [BindNever]
        public List<VendorModel> Vendors { get; set; }
        public List<PurchaseOrderItemModel> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public int OrderStatus { get; set; } = 0; 

    }

    public class PurchaseOrderItemModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public decimal Price { get; set; }

        public string? ProductName { get; set; }
    }
}
