using LoginPanel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace LoginPanel.Controllers
{
	
	public class VendorProductPricingController : Controller
    {
        private readonly IVendorProductPriceService _service;

		#region VENDOR PRODUCT PRICING CONTROLLER
		public VendorProductPricingController(IVendorProductPriceService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new VendorProductPricingCombinedModel
            {
                VendorList = _service.GetActiveVendors(),
                ProductList = _service.GetActiveProducts(),
                VendorProductPrices = _service.GetAllVendorProductPrices()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult PrtVendorPricingList()
        {
            var list = _service.GetAllVendorProductPrices();
            return PartialView("PrtVendorPricingList", list);
        }


        [HttpPost]
        public IActionResult Save([FromBody] VendorProductPricingReq req)
        {
            if (req == null)
                return Json(new { success = false });

            _service.SaveVendorProductPrice(req);

            return Json(new { success = true });
        }
		[HttpGet]
		public IActionResult HistoryPopup(int vendorId, int productId)
		{
			var history = _service.GetVendorProductPriceHistory(vendorId, productId);
			return PartialView("_VendorProductPricingHistory", history);
		}
		#endregion
	}
}