using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace LoginPanel.Models
{
    public class InventoryAdjustmentDto
    {
        // 🔹 Dropdown ke liye
        public int ProductId { get; set; }
        public List<SelectListItem> ProductList { get; set; }

        // 🔹 Inventory se aane wali values
        public string ProductName { get; set; }
        public int CurrentQty { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }

        // 🔹 Admin input
        public int QtyChange { get; set; }   // + ya -
        public int FinalQty { get; set; }

        // 🔹 Reason
        public int ReasonId { get; set; }
        public List<SelectListItem> ReasonList { get; set; }
    }
}
