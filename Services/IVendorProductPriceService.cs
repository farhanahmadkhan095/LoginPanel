using System.Collections.Generic;
using LoginPanel.Models;


public interface IVendorProductPriceService
{
    List<VendorModel> GetActiveVendors();
    List<ProductModel> GetActiveProducts();
    void SaveVendorProductPrice(VendorProductPricingReq req);
    List<VendorProductPriceDisplay> GetAllVendorProductPrices();
	List<VendorProductPricingHistoryModel> GetVendorProductPriceHistory(int vendorId, int productId);
}
