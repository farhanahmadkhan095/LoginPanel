using Microsoft.AspNetCore.Mvc;
using LoginPanel.Services;
using LoginPanel.Models;

namespace LoginPanel.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }


        public async Task<IActionResult> Index()
        {
            var data = await _inventoryService.GetAllAsync();
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Adjustment(int? productId)
        {
            ViewBag.Products = await _inventoryService.GetProductsAsync();
            ViewBag.Reasons = await _inventoryService.GetReasonsAsync();

            if (productId.HasValue && productId > 0)
            {
                var model = await _inventoryService.GetInventoryByProductAsync(productId.Value);
                return View(model);
            }

            return View(new InventoryAdjustmentDto());
        }


        [HttpPost]
        public async Task<IActionResult> AdjustmentSave(int productId, int qtyChange, int reasonId)
        {
            try
            {
                await _inventoryService.AdjustInventoryAsync(productId, qtyChange, reasonId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentAdjustments(int productId)
        {
            var history = await _inventoryService.GetRecentAdjustmentsAsync(productId);
            return PartialView("_InventoryAdjustmentHistory", history);
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentQty(int productId)
        {
            var inventory = await _inventoryService.GetInventoryByProductAsync(productId);
            return Json(inventory?.CurrentQty ?? 0);
        }
        [HttpGet]
        public async Task<IActionResult> Correction()
        {
            var model = new InventoryCorrectionVM
            {
                ProductList = await _inventoryService.GetProductsAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CorrectInventory(int ProductId, int NewQuantity, int ReasonId)
        {
            if (ProductId <= 0 || NewQuantity < 0)
                return Json(new { success = false, message = "Invalid data" });

            try
            {
                await _inventoryService.CorrectInventoryAsync(
                    ProductId,
                    NewQuantity,
                    ReasonId,
                    "Manual inventory correction"   
                );

                return Json(new
                {
                    success = true,
                    message = "Inventory corrected successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public async Task<IActionResult> History()
        {
            ViewBag.Products = await _inventoryService.GetProductsAsync();
            return View();
        }

    
        [HttpGet]
        public async Task<IActionResult> GetInventoryHistory(int productId)
        {
            var data = await _inventoryService.GetInventoryHistoryAsync(productId);
            return PartialView("_InventoryHistoryTable", data);
        }
    }
}
