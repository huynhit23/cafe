using cafe.Data;
using cafe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace cafe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // BLOG LIST
        // =============================
        public IActionResult Index(string? title, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var blogs = _context.Blogs
                .OrderByDescending(b => b.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                blogs = blogs.Where(b => b.Title.Contains(title));
            }

            return View(blogs.ToPagedList(pageNumber, pageSize));
        }

        // =============================
        // DETAILS
        // =============================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null) return NotFound();

            return View(blog);
        }

        // =============================
        // CREATE GET
        // =============================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =============================
        // CREATE POST
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog, IFormFile? ImageFile)
        {
            TempData["Message"] = "";

            var checkBlog = _context.Blogs
                .FirstOrDefault(b => b.Title == blog.Title);

            if (checkBlog != null)
            {
                ViewBag.error = "Blog title already exists";
                return View(blog);
            }

            if (ModelState.IsValid)
            {
                blog.CreatedDate = DateTime.Now;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

                    var folder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "Images"
                    );

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    var path = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    blog.ImageUrl = fileName;
                }

                _context.Add(blog);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Blog added successfully";

                return RedirectToAction(nameof(Index));
            }

            return View(blog);
        }

        // =============================
        // EDIT GET
        // =============================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _context.Blogs.FindAsync(id);

            if (blog == null) return NotFound();

            return View(blog);
        }

        // =============================
        // EDIT POST
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog, IFormFile? ImageFile, string? OldImage)
        {
            TempData["Message"] = "";

            if (id != blog.Id) return NotFound();

            var checkBlog = _context.Blogs
                .FirstOrDefault(b => b.Title == blog.Title && b.Id != blog.Id);

            if (checkBlog != null)
            {
                ViewBag.error = "Blog title already exists";
                return View(blog);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

                        var folder = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "Images"
                        );

                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        var path = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        blog.ImageUrl = fileName;
                    }
                    else
                    {
                        blog.ImageUrl = OldImage;
                    }

                    _context.Update(blog);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Blog updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(blog);
        }

        // =============================
        // DELETE
        // =============================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            TempData["Message"] = "";

            var blog = await _context.Blogs.FindAsync(id);

            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Blog deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // CHECK BLOG
        // =============================
        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(b => b.Id == id);
        }
    }
}