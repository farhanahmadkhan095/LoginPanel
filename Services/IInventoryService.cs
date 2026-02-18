using Microsoft.AspNetCore.Mvc.Rendering;
using LoginPanel.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoginPanel.Services
{
    public interface IInventoryService
    {
        // 🔹 Inventory List (Current only)
        Task<IEnumerable<InventoryDto>> GetAllAsync();

        // 🔹 Dropdowns
        Task<IEnumerable<SelectListItem>> GetProductsAsync();
        Task<IEnumerable<SelectListItem>> GetReasonsAsync();

        // 🔹 Inventory Detail by Product (CURRENT QTY)
        Task<InventoryAdjustmentDto> GetInventoryByProductAsync(int productId);

        // 🔹 Sale / Purchase Adjustment (+ / -)
        Task AdjustInventoryAsync(int productId, int qtyChange, int reasonId);

        // 🔹 Recent Adjustments (history only, no base row)
        Task<IEnumerable<InventoryAdjustmentHistoryDto>> GetRecentAdjustmentsAsync(int productId);

        // 🔹 Current Quantity (FINAL value, no calculation)
        Task<int> GetCurrentQtyAsync(int productId);

        // 🔹 Inventory Correction (BASE QTY REPLACE)
        Task CorrectInventoryAsync(int productId, int newQuantity, int reasonId, string correctionReason);

        Task<IEnumerable<InventoryHistoryVM>> GetInventoryHistoryAsync(int productId);

    }
}
