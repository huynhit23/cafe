using cafe.Data;
using cafe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace cafe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LIST + SEARCH + PAGINATION
        public IActionResult Index(string? name, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var categories = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                categories = categories.Where(c => c.Name.Contains(name));
            }

            var result = categories
                .OrderByDescending(c => c.Id)
                .ToPagedList(pageNumber, pageSize);

            return View(result);
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // CREATE
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            TempData["Message"] = "";
            TempData["MessageError"] = "";

            var check = _context.Categories
                .FirstOrDefault(c => c.Name == category.Name);

            if (check != null)
            {
                ViewBag.error = "Tên danh mục đã tồn tại";
                return View(category);
            }

            if (ModelState.IsValid)
            {
                category.CreatedDate = DateTime.Now;

                _context.Add(category);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Thêm danh mục thành công";

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);

            if (category == null) return NotFound();

            return View(category);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            TempData["Message"] = "";
            TempData["MessageError"] = "";

            var check = _context.Categories
                .FirstOrDefault(c => c.Name == category.Name && c.Id != category.Id);

            if (check != null)
            {
                ViewBag.error = "Tên danh mục đã tồn tại";
                return View(category);
            }

            if (id != category.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Cập nhật danh mục thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            TempData["Message"] = "";
            TempData["MessageError"] = "";

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            TempData["Message"] = "Xóa danh mục thành công";

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}