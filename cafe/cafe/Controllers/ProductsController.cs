using cafe.Data;
using cafe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cafe.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductsController(ApplicationDbContext db) { _db = db; }

        // GET /Products
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            var products = await GetFilteredProducts(search, categoryId);

            ViewBag.Categories = categories;
            ViewBag.Search = search;
            ViewBag.SelectedCategory = categoryId;
            return View(products);
        }

        // GET /Products/Filter (Ajax)
        public async Task<IActionResult> Filter(string? search, int? categoryId)
        {
            var products = await GetFilteredProducts(search, categoryId);
            return PartialView("_ProductListPartial", products);
        }

        private async Task<List<Product>> GetFilteredProducts(string? search, int? categoryId)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            return await query.OrderByDescending(p => p.CreatedDate).ToListAsync();
        }

        // GET /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
