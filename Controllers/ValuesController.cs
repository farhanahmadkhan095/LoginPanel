using LoginPanel.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoginPanel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IReceivedPurchaseService _service;

        public  ValuesController(IReceivedPurchaseService receivedPurchase)
        {
            _service = receivedPurchase;
        }
        [HttpPost]
        public IActionResult Details(int id)
        {
            var model = _service.GetReceivePageData(id);
            return Ok(model);
        }
    }
}
