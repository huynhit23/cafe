using cafe.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cafe.Controllers
{
    public class BlogsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public BlogsController(ApplicationDbContext db) { _db = db; }

        // GET /Blogs
        public async Task<IActionResult> Index()
        {
            var blogs = await _db.Blogs
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
            return View(blogs);
        }

        // GET /Blogs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _db.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && b.IsPublished);
            
            if (blog == null) return NotFound();
            
            return View(blog);
        }
    }
}
