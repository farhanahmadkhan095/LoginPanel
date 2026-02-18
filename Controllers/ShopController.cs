using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using LoginPanel.Models;
using LoginPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;


namespace LoginPanel.Controllers

{
    public class ShopController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _service;
        private readonly string _cs;
        private readonly IConfiguration _config;
        private const string ADDR_KEY = "ADDR";


        public ShopController(IProductService service, IOrderService orderService, IConfiguration cfg)
        {
            _service = service;
            _orderService = orderService;
            _config = cfg;
            _cs = cfg.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IActionResult> Index(string q = "")
        {
            var products = await _service.GetProductsAsync();

            q = (q ?? "").Trim();
            while (q.Contains("  ")) q = q.Replace("  ", " ");
            var key = q.ToLower();

            if (!string.IsNullOrWhiteSpace(key))
            {
                products = products
                    .Where(p =>
                        ((p.ProductName ?? "").ToLower().Contains(key)) ||
                        ((p.ProductDescription ?? "").ToLower().Contains(key))
                    )
                    .ToList();
            }

            foreach (var p in products)
                p.AvailableQty = await _service.GetStockAsync(p.ProductId);

            ViewBag.Search = q;
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _service.GetProductAsync(id);
            if (product is null) return NotFound();
            return View(product);
        }
        public async Task<IActionResult> BuyNow(int id)
        {
            // login gate
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "LoginPanel", new { returnUrl = $"/Shop/Details/{id}" });

            var product = await _service.GetProductAsync(id);
            if (product == null) return NotFound();

            // latest stock
            var stock = await _service.GetStockAsync(id);
            if (stock <= 0)
            {
                TempData["Error"] = "Out of stock!";
                return RedirectToAction("Details", new { id });
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.ProductId == id);

            if (existing == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice,
                    Qty = 1,
                    AvailableQty = stock
                });
            }
            else
            {
                if (existing.Qty + 1 > stock)
                {
                    TempData["Error"] = "Stock limit reached!";
                    return RedirectToAction("Cart");
                }

                existing.Qty += 1;
                existing.AvailableQty = stock;
            }

            SaveCart(cart);
            TempData["Success"] = "Added to cart!";
            return RedirectToAction("Cart"); // ✅ yahi chahiye tumhe
        }

        public async Task<IActionResult> Checkout(int productId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "LoginPanel", new { returnUrl = $"/Shop/Checkout?productId={productId}" });

            var product = await _service.GetProductAsync(productId);
            if (product == null) return NotFound();

            // current stock (IsCurrent=1)
            int stock = 0;
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 Quantity 
                FROM dbo.Inventory 
                WHERE ProductId=@ProductId AND IsCurrent=1
                ORDER BY InventoryId DESC;", con))
            {
                cmd.Parameters.AddWithValue("@ProductId", productId);
                con.Open();
                var obj = cmd.ExecuteScalar();
                stock = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
            }

            ViewBag.Stock = stock;
            return View(product); // Views/Shop/Checkout.cshtml
        }

        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "LoginPanel", new { returnUrl = "/Shop/Profile" });

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.RoleName = HttpContext.Session.GetString("RoleName");

         
            var addr = await _orderService.GetProfileAddressAsync(userId);
            HttpContext.Session.SetString(
            "ADDR",
            JsonSerializer.Serialize(addr)
        );
            return View(addr); 
        }

        public async Task<IActionResult> Orders()
        
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "LoginPanel", new { returnUrl = "/Shop/Orders" });

            var orders = await _orderService.GetUserOrdersAsync(userId.Value);
            return View(orders); // Views/Shop/Orders.cshtml
        }
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "LoginPanel", new { returnUrl = $"/Shop/OrderDetails/{id}" });

            var model = await _orderService.GetUserOrderDetailsAsync(userId.Value, id);
            if (model == null) return NotFound();

            return View(model); // Views/Shop/OrderDetails.cshtml
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString("CART");
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("CART", JsonSerializer.Serialize(cart));
        }

        public async Task<IActionResult> AddToCart(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "LoginPanel", new { returnUrl = $"/Shop/Details/{id}" });

            var product = await _service.GetProductAsync(id);
            if (product == null) return NotFound();

            var stock = await _service.GetStockAsync(id);
            if (stock <= 0)
            {
                TempData["Error"] = "Out of stock!";
                return RedirectToAction("Details", new { id });
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.ProductId == id);

            if (existing == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice,
                    Qty = 1,
                    AvailableQty = stock
                });
            }
            else
            {
                if (existing.Qty + 1 > stock)
                {
                    TempData["Error"] = "Stock limit reached!";
                    return RedirectToAction("Cart");
                }

                existing.Qty += 1;
                existing.AvailableQty = stock;
            }

            SaveCart(cart);
            TempData["Success"] = "Added to cart!";
            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> Cart()
        {
            var cart = GetCart();

            CheckoutAddressVM addr = null;
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                using var con = new SqlConnection(_cs);

                string sql = @"
            SELECT TOP 1 FullName, Mobile, AddressLine, City, Pincode
            FROM UserAddress
            WHERE UserId = @UserId
            ORDER BY CreatedDate DESC
        ";

                addr = await con.QueryFirstOrDefaultAsync<CheckoutAddressVM>(sql, new
                {
                    UserId = userId.Value
                });
            }

            ViewBag.Address = addr;   
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CartPlus(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return RedirectToAction("Cart");

            var stock = await _service.GetStockAsync(productId);
            if (item.Qty + 1 > stock)
            {
                TempData["Error"] = "Stock limit reached!";
                return RedirectToAction("Cart");
            }

            item.Qty += 1;
            item.AvailableQty = stock;

            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CartMinus(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return RedirectToAction("Cart");

            item.Qty -= 1;
            if (item.Qty <= 0)
                cart.Remove(item);

            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductId == productId);
            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("CART");
            return RedirectToAction("Cart");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CartCheckout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "LoginPanel", new { returnUrl = "/Shop/Cart" });

            var cart = GetCart();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Cart is empty!";
                return RedirectToAction("Cart");
            }

            var dt = new DataTable();
            dt.Columns.Add("ProductId", typeof(int));
            dt.Columns.Add("Qty", typeof(int));

            foreach (var item in cart)
            {
                var stock = await _service.GetStockAsync(item.ProductId);
                if (stock < item.Qty)
                {
                    TempData["Error"] = $"Insufficient stock for {item.ProductName}. Available: {stock}";
                    return RedirectToAction("Cart");
                }
                dt.Rows.Add(item.ProductId, item.Qty);
            }

            using var con = new SqlConnection(_cs);

            var p = new DynamicParameters();
            p.Add("@UserId", userId.Value);
            p.Add("@Items", dt.AsTableValuedParameter("dbo.OrderItemType"));

            var result = await con.QueryFirstAsync<dynamic>(
                "dbo.usp_PlaceOrder",
                p,
                commandType: CommandType.StoredProcedure
            );

            int orderId = (int)result.OrderId;

            // cart clear (order placed)
            HttpContext.Session.Remove("CART");

            // ✅ now open payment options page
            return RedirectToAction("Pay", new { orderId = orderId });
        }

        [HttpGet]
        public IActionResult New()
        {
            return View(); // Views/Shop/New.cshtml
        }

        [HttpGet]
        public IActionResult Offers()
        {
            return View(); // Views/Shop/Offers.cshtml
        }

        [HttpGet]
        public IActionResult Delivery()
        {
            return View(); // Views/Shop/Delivery.cshtml
        }

        [HttpGet]
        public IActionResult Support()
        {
            return View(); // Views/Shop/Support.cshtml
        }
        [HttpGet]
        public async Task<IActionResult> Pay(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "LoginPanel", new { returnUrl = $"/Shop/Pay?orderId={orderId}" });

            var order = await _orderService.GetOrderSummaryByOrderIdAsync(orderId);

            if (order == null) return NotFound();

            var vm = new LoginPanel.Models.PaymentViewModel
            {
                OrderId = order.OrderId,
              
                 Amount = order.TotalAmount
            };

            return View(vm); // Views/Shop/Pay.cshtml
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayConfirm(int orderId, string paymentMode)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "LoginPanel");

            if (paymentMode != "COD")
                return RedirectToAction("Pay", new { orderId });

            await _orderService.SavePaymentAsync(orderId, userId.Value, "COD", null);
            await _service.DeductInventoryAfterPaymentAsync(orderId);

            using var con = new SqlConnection(_cs);

                    string sql = @"
                SELECT TOP 1 FullName, Mobile, AddressLine, City, Pincode
                FROM UserAddress
                WHERE UserId = @UserId
                ORDER BY CreatedDate DESC
            ";

            var addr = await con.QueryFirstOrDefaultAsync<CheckoutAddressVM>(sql, new
            {
                UserId = userId.Value
            });

            if (addr != null)
            {
                await _orderService.SaveOrderAddressAsync(orderId, addr);
            }

            TempData["Success"] = "Order placed with Cash on Delivery";
            return RedirectToAction("Orders");
        }

        [HttpPost]
        public async Task<IActionResult> CreateRazorpayOrder([FromBody] int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { error = "SESSION_EXPIRED" });

            var order = await _orderService.GetOrderSummaryByOrderIdAsync(orderId);
            if (order == null)
                return Json(new { error = "ORDER_NOT_FOUND" });

            string key = _config["Razorpay:Key"];
            string secret = _config["Razorpay:Secret"];

            var client = new Razorpay.Api.RazorpayClient(key, secret);

            int amountInPaise = Convert.ToInt32(order.TotalAmount * 100);

            var options = new Dictionary<string, object>
    {
        { "amount", amountInPaise },
        { "currency", "INR" },
        { "receipt", $"order_{orderId}" }
    };

            var rpOrder = client.Order.Create(options);

            return Json(new
            {
                key = key,
                orderId = rpOrder["id"],
                amount = amountInPaise
            });
        }
        

        [HttpPost]
        public async Task<IActionResult> RazorpayVerify(int orderId, string razorpay_payment_id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "LoginPanel");

            // 1️⃣ Payment id empty = FAILED
            if (string.IsNullOrWhiteSpace(razorpay_payment_id))
            {
                TempData["Error"] = "Payment failed or cancelled.";
                return RedirectToAction("Pay", new { orderId });
            }

            // 2️⃣ SAVE PAYMENT (ONLINE)
            await _orderService.SavePaymentAsync(
                orderId,
                userId.Value,
                "ONLINE",
                razorpay_payment_id
            );

            // 3️⃣ INVENTORY KAM KARO (PAYMENT KE BAAD)
            await _service.DeductInventoryAfterPaymentAsync(orderId);

            //// 4️⃣ ADDRESS SAVE (AGAR SESSION ME HAI)
            //var addrJson = HttpContext.Session.GetString("ADDR");
            //if (!string.IsNullOrEmpty(addrJson))
            //{
            //    var addr = JsonSerializer.Deserialize<CheckoutAddressVM>(addrJson);
            //    if (addr != null)
            //    {
            //        await _orderService.SaveOrderAddressAsync(orderId, addr);
            //        HttpContext.Session.Remove("ADDR"); // cleanup
            //    }
            //}

            TempData["Success"] = "Payment successful!";
            return RedirectToAction("Orders");
        }



        [HttpGet]
        public async Task<IActionResult> CheckoutAddress()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "LoginPanel");

            using var con = new SqlConnection(_cs);

            string sql = @"
        SELECT TOP 1 FullName, Mobile, AddressLine, City, Pincode
        FROM UserAddress
        WHERE UserId = @UserId
        ORDER BY CreatedDate DESC
    ";

            var vm = await con.QueryFirstOrDefaultAsync<CheckoutAddressVM>(sql, new
            {
                UserId = userId.Value
            });

            return View(vm ?? new CheckoutAddressVM());
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutAddress(CheckoutAddressVM model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "LoginPanel");

            if (!ModelState.IsValid)
                return View(model);

            using var con = new SqlConnection(_cs);

            string sql = @"
        IF NOT EXISTS (SELECT 1 FROM UserAddress WHERE UserId=@UserId)
        BEGIN
            INSERT INTO UserAddress
            (UserId, FullName, Mobile, AddressLine, City, Pincode, CreatedDate)
            VALUES
            (@UserId, @FullName, @Mobile, @AddressLine, @City, @Pincode, GETDATE())
        END
        ELSE
        BEGIN
            UPDATE UserAddress
            SET FullName=@FullName,
                Mobile=@Mobile,
                AddressLine=@AddressLine,
                City=@City,
                Pincode=@Pincode
            WHERE UserId=@UserId
        END
    ";

            await con.ExecuteAsync(sql, new
            {
                UserId = userId.Value,
                model.FullName,
                model.Mobile,
                model.AddressLine,
                model.City,
                model.Pincode
            });

            // 👉 Address permanent save ho gaya
            return RedirectToAction("Cart");
        }

        // Wallet Page
        public IActionResult Wallet()
        {
            // demo wallet balance
            if (HttpContext.Session.GetInt32("WALLET") == null)
                HttpContext.Session.SetInt32("WALLET", 500);

            return View();
        }

        // Wishlist Page (demo)
        public IActionResult Wishlist()
        {
            return View();
        }
        
        public IActionResult Result(int id)
        {
            PaymentViewModel payment;

            using (var con = new SqlConnection(_cs))
            {
                string sql = @"SELECT Id, Amount, PaymentStatus
                       FROM Payments
                       WHERE Id = @Id";

                payment = con.QueryFirstOrDefault<PaymentViewModel>(sql, new { Id = id });
            }

            if (payment == null)
                return NotFound();

            return View(payment); // Views/Shop/Result.cshtml
        }
        [HttpPost]
        public async Task<IActionResult> PaymentFailed([FromBody] int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            await _orderService.SavePaymentAsync(
                orderId,
                userId.Value,
                "ONLINE",
                "FAILED"
            );

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "LoginPanel");

            await _orderService.CancelOrderAsync(orderId, userId.Value);

            return RedirectToAction("OrderDetails", new { id = orderId });
        }


    }
}