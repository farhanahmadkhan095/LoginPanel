using LoginPanel.Models;
using LoginPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoginPanel.Controllers
{
    public class InventoryAdjustmentReasonController : Controller
    {
        private readonly IInventoryAdjustmentReasonService _service;

        public InventoryAdjustmentReasonController(IInventoryAdjustmentReasonService service)
        {
            _service = service;
        }

        #region INVENTORY REASON MASTER

        public async Task<IActionResult> Index()
        {
            var list = await _service.GetAllAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            return View("Form", new InventoryAdjustmentReasonDto());
        }

        public async Task<IActionResult> Edit(int id)
        {
            var data = await _service.GetByIdAsync(id);

            if (data == null)
                return NotFound();

            return View("Form", data);
        }

        [HttpPost]
        public async Task<IActionResult> Save(InventoryAdjustmentReasonDto model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            await _service.AddOrUpdateAsync(model);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            await _service.ToggleActiveAsync(id, isActive);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ToggleDeleted(int id, bool isDeleted)
        {
            await _service.ToggleDeletedAsync(id, isDeleted);
            return RedirectToAction("Index");
        }

        #endregion

        #region INVENTORY ADJUSTMENT

        #endregion

    }
}
