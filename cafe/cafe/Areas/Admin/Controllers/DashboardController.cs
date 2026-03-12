using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cafe.Data;
using Microsoft.EntityFrameworkCore;

namespace cafe.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalBlogs = await _context.Blogs.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            return View();
        }
    }
}
