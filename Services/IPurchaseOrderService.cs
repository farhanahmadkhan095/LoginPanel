using LoginPanel.Models;
using System.Collections.Generic;

public interface IPurchaseOrderService
{
	PurchaseOrderViewModel GetCreatePageData();

	bool CreatePurchaseOrder(PurchaseOrderViewModel model);

	List<PurchaseOrderItemModel> GetVendorProductsWithPrice(int vendorId);
}