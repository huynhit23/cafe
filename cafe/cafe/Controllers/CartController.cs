using cafe.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace cafe.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const string CartKey = "cart";

        public CartController(ApplicationDbContext db) { _db = db; }

        private List<CartItem> GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CartKey);
            return string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new();
        }
        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));
            var total = cart.Sum(i => i.Quantity);
            HttpContext.Session.Set("CartCount", BitConverter.GetBytes(total));
        }

        // POST /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddToCartRequest req)
        {
            var product = await _db.Products.FindAsync(req.ProductId);
            if (product == null) return NotFound();

            var cart = GetCartFromSession();
            var existing = cart.FirstOrDefault(x => x.ProductId == req.ProductId);
            if (existing != null)
                existing.Quantity += req.Quantity;
            else
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl ?? "",
                    Quantity = req.Quantity
                });

            SaveCart(cart);
            return Json(new { totalQty = cart.Sum(i => i.Quantity) });
        }

        // POST /Cart/Remove/{id}
        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cart = GetCartFromSession();
            cart.RemoveAll(x => x.ProductId == id);
            SaveCart(cart);
            return Ok();
        }

        // POST /Cart/UpdateQty
        [HttpPost]
        public IActionResult UpdateQty([FromBody] UpdateQtyRequest req)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(x => x.ProductId == req.ProductId);
            if (item != null)
            {
                item.Quantity += req.Delta;
                if (item.Quantity <= 0) cart.Remove(item);
            }
            SaveCart(cart);
            return Ok();
        }

        // GET /Cart/GetCart
        [HttpGet]
        public IActionResult GetCart()
        {
            var cart = GetCartFromSession();
            return Json(new
            {
                items = cart,
                totalQty = cart.Sum(i => i.Quantity),
                totalPrice = cart.Sum(i => i.Price * i.Quantity)
            });
        }

        // POST /Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartKey);
            HttpContext.Session.Remove("CartCount");
            return Ok();
        }
    }

    // DTOs
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = "";
        public int Quantity { get; set; }
    }
    public class AddToCartRequest { public int ProductId { get; set; } public int Quantity { get; set; } = 1; }
    public class UpdateQtyRequest { public int ProductId { get; set; } public int Delta { get; set; } }
}
