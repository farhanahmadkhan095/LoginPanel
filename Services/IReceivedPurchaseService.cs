using LoginPanel.Models;
using System.Collections.Generic;

namespace LoginPanel.Services
{
    public interface IReceivedPurchaseService
    {
        List<PurchaseOrderViewModel> GetPendingPurchaseOrders();
        List<PurchaseOrderViewModel> GetAllPurchaseOrders();
        ReceivePurchaseViewModel GetReceivePageData(int poId);
        void SaveReceivedItems(ReceivePurchaseViewModel model, int userId);

        bool IsPurchaseOrderReceived(int purchaseOrderId);
    }
}
