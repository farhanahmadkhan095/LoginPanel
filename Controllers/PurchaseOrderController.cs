using LoginPanel.Models;
using Microsoft.AspNetCore.Mvc;

public class PurchaseOrderController : Controller
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrderController(IPurchaseOrderService service)
    {
        _service = service;
    }

 
    [HttpGet]
    public IActionResult Create()
    {
        var model = _service.GetCreatePageData();
        return View(model);
    }

   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(PurchaseOrderViewModel model)
    {
        model.OrderStatus = 0;

        if (model.SelectedVendorId == 0)
        {
            ModelState.AddModelError("", "Please select vendor");
        }
       
        if (model.Items == null || model.Items.Count == 0)
        {
            ModelState.AddModelError("", "Please add at least one product");
        }

        bool success = _service.CreatePurchaseOrder(model);
        if (!ModelState.IsValid)
        {
            model.Vendors = _service.GetCreatePageData().Vendors;
            return View(model);
        }

        if (success)
        {
            TempData["Success"] = "Purchase Order created successfully";
            return RedirectToAction("Create");
        }

      
        ModelState.AddModelError("", "Failed to create purchase order");
        model.Vendors = _service.GetCreatePageData().Vendors;
        return View(model);
    }

    
    [HttpGet]
    public IActionResult GetVendorProducts(int vendorId)
    {
        var data = _service.GetVendorProductsWithPrice(vendorId);
        return Json(data);
    }
}
