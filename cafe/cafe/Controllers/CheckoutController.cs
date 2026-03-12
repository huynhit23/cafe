using cafe.Data;
using cafe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using cafe.Hubs;
using cafe.Services;
using System.Text.Json;

namespace cafe.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly IVnPayService _vnPayService;
        private readonly IEmailService _emailService;
        private const string CartKey = "cart";

        public CheckoutController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IHubContext<OrderHub> hubContext, IVnPayService vnPayService, IEmailService emailService)
        {
            _db = db;
            _userManager = userManager;
            _hubContext = hubContext;
            _vnPayService = vnPayService;
            _emailService = emailService;
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartKey);
            return string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new();
        }

        // GET /Checkout
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Index", "Products");

            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserPoints = user?.LoyaltyPoints ?? 0;
            ViewBag.Total = cart.Sum(i => i.Price * i.Quantity);
            return View(cart);
        }

        // POST /Checkout/Confirm
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(string shippingAddress, string paymentMethod, int usedPoints = 0)
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Index", "Products");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            decimal totalAmount = cart.Sum(i => i.Price * i.Quantity);
            decimal discount = 0;

            // Redeem Points: 1 point = 1000 VND
            if (usedPoints > 0 && user.LoyaltyPoints >= usedPoints)
            {
                discount = usedPoints * 1000;
                if (discount > totalAmount) discount = totalAmount;
                user.LoyaltyPoints -= usedPoints;
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount - discount,
                Status = "Pending",
                ShippingAddress = shippingAddress,
                PaymentStatus = "Unpaid"
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Earn Points: 1 point per 10,000 VND (on final paid amount)
            // Note: points are awarded now, but practically should be awarded after payment.
            // For simplicity in this demo, we add them now.
            user.LoyaltyPoints += (int)((totalAmount - discount) / 10000);
            await _userManager.UpdateAsync(user);

            foreach (var item in cart)
            {
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Size = item.Size,
                    SugarLevel = item.SugarLevel,
                    IceLevel = item.IceLevel
                });
            }
            await _db.SaveChangesAsync();

            // Xóa giỏ hàng
            HttpContext.Session.Remove(CartKey);
            HttpContext.Session.Remove("CartCount");

            // ✅ EMAIL NOTIFICATION
            string emailBody = $"<h3>Xác nhận đơn hàng #{order.Id}</h3>" +
                               $"<p>Chào {user.FullName}, cảm ơn bạn đã đặt hàng tại Cafe Providence!</p>" +
                               $"<p>Tổng tiền: {(totalAmount - discount).ToString("N0")} ₫</p>" +
                               $"<p>Địa chỉ: {shippingAddress}</p>";
            await _emailService.SendEmailAsync(user.Email!, $"[Cafe Providence] Xác nhận đơn hàng #{order.Id}", emailBody);

            // ✅ SIGNALR NOTIFICATION
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNewOrder", user.FullName, order.TotalAmount.ToString("N0"), order.Id);
            }
            catch { }

            if (paymentMethod == "VNPay")
            {
                var vnpayModel = new VnPayRequestModel
                {
                    Amount = (double)order.TotalAmount,
                    CreatedDate = DateTime.Now,
                    Description = $"{user.FullName} thanh toan don hang {order.Id}",
                    FullName = user.FullName ?? "Customer",
                    OrderId = order.Id
                };
                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnpayModel));
            }

            TempData["OrderId"] = order.Id;
            return RedirectToAction("Success");
        }

        public async Task<IActionResult> VnPayReturn()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VNPay: {response?.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            // Thanh toán thành công
            var orderIdStr = response.OrderId;
            if (int.TryParse(orderIdStr, out int orderId))
            {
                var order = await _db.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.PaymentStatus = "Paid";
                    await _db.SaveChangesAsync();
                }
                TempData["OrderId"] = orderId;
            }
            
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            var orderId = TempData["OrderId"];
            ViewBag.OrderId = orderId;
            return View();
        }

        public IActionResult PaymentFail() => View();
    }
}
