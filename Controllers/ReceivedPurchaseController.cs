using Microsoft.AspNetCore.Mvc;
using LoginPanel.Services;
using LoginPanel.Models;

public class ReceivedPurchaseController : Controller
{
    private readonly IReceivedPurchaseService _service;

    public ReceivedPurchaseController(IReceivedPurchaseService service)
    {
        _service = service;
    }


    public IActionResult Index()
    {
        var list = _service.GetAllPurchaseOrders();
        return View(list);
    }


    [HttpGet]
    public IActionResult Receive(int id)
    {
        
        if (_service.IsPurchaseOrderReceived(id))
        {
            TempData["ErrorMessage"] = "This purchase order is already received.";
            return RedirectToAction("Index");
        }

        var model = _service.GetReceivePageData(id);
        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Receive(ReceivePurchaseViewModel model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "LoginPanel");

        if (model.Items == null || model.Items.Count == 0)
        {
            ModelState.AddModelError("", "No items to receive");
            return View(model);
        }

        foreach (var item in model.Items)
        {
            if (item.ReceivedQty <= 0 || item.ReceivedQty > item.OrderedQty)
            {
                ModelState.AddModelError("",
                    "Received quantity must be greater than 0 and less than or equal to ordered quantity");
                return View(model);
            }
        }

        _service.SaveReceivedItems(model, userId.Value);

        TempData["SuccessMessage"] = $"PO #{model.PurchaseOrderId} received successfully!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var model = _service.GetReceivePageData(id);
        return PartialView("_ReceiveDetailsPartial", model);
    }
}
