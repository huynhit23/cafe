using cafe.Data;
using cafe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace cafe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // PRODUCT LIST
        // =============================
        public IActionResult Index(string? name, int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var products = _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                products = products.Where(p => p.Name.Contains(name));
            }

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // =============================
        // DETAILS
        // =============================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // =============================
        // CREATE GET
        // =============================
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // =============================
        // CREATE POST
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            TempData["Message"] = "";

            var checkProduct = _context.Products
                .FirstOrDefault(p => p.Name == product.Name);

            if (checkProduct != null)
            {
                ViewBag.error = "Product name already exists";
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }

            if (ModelState.IsValid)
            {
                product.CreatedDate = DateTime.Now;

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

                    product.ImageUrl = fileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Product added successfully";

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        // =============================
        // EDIT GET
        // =============================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);

            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        // =============================
        // EDIT POST
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? ImageFile, string? OldImage)
        {
            TempData["Message"] = "";

            if (id != product.Id) return NotFound();

            var checkProduct = _context.Products
                .FirstOrDefault(p => p.Name == product.Name && p.Id != product.Id);

            if (checkProduct != null)
            {
                ViewBag.error = "Product name already exists";
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
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

                        product.ImageUrl = fileName;
                    }
                    else
                    {
                        product.ImageUrl = OldImage;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Product updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        // =============================
        // DELETE
        // =============================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            TempData["Message"] = "";

            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Product deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // CHECK PRODUCT
        // =============================
        private bool ProductExists(int id)
        {
            return _context.Products.Any(p => p.Id == id);
        }
    }
}