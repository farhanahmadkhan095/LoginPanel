using System.Collections.Generic;

namespace LoginPanel.Models
{
    public class VendorProductPricingCombinedModel
    {
        public List<VendorModel> VendorList { get; set; } 
        public List<ProductModel> ProductList { get; set; }
        public List<VendorProductPriceDisplay> VendorProductPrices { get; set; }
        public bool IsActive { get; set; } = true;


        //public List<VendorModel> Vendors { get; set; }
        //public List<ProductModel> Products { get; set; }
       
    }

    public class VendorProductPricingReq
    {

        public int  VendorId { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }

    }
}
namespace LoginPanel.Models
{
    public class VendorProductPriceDisplay
    {
        public int VendorProductPriceId { get; set; }
		public int VendorId { get; set; }
		public int ProductId { get; set; }
		public string VendorName { get; set; }
        public string ProductName { get; set; }
        public decimal MasterPrice { get; set; }
        public decimal VendorPrice { get; set; }
        public bool IsActive { get; set; }
    }
}
namespace LoginPanel.Models
{
	public class VendorProductPricingHistoryModel
	{
		public int HistoryId { get; set; }
		public string VendorName { get; set; }
		public string ProductName { get; set; }
		public decimal OldPrice { get; set; }
		public decimal NewPrice { get; set; }
		public DateTime ChangedOn { get; set; }
	}
}