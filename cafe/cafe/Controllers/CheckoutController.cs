using cafe.Data;
using cafe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace cafe.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CartKey = "cart";

        public CheckoutController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartKey);
            return string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new();
        }

        // GET /Checkout
        public IActionResult Index()
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Index", "Products");

            ViewBag.Total = cart.Sum(i => i.Price * i.Quantity);
            return View(cart);
        }

        // POST /Checkout/Confirm
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(string shippingAddress)
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Index", "Products");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(i => i.Price * i.Quantity),
                Status = "Pending",
                ShippingAddress = shippingAddress,
                PaymentStatus = "Unpaid"
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var item in cart)
            {
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }
            await _db.SaveChangesAsync();

            // Xóa giỏ hàng
            HttpContext.Session.Remove(CartKey);
            HttpContext.Session.Remove("CartCount");

            TempData["OrderId"] = order.Id;
            return RedirectToAction("Success");
        }

        // GET /Checkout/Success
        public IActionResult Success()
        {
            var orderId = TempData["OrderId"];
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}
