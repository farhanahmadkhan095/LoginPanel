using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace LoginPanel.Models
{
    public class InventoryCorrectionVM
    {
        public int ProductId { get; set; }
        public int NewQuantity { get; set; }

        public IEnumerable<SelectListItem> ProductList { get; set; }
            = new List<SelectListItem>();
    }
}
