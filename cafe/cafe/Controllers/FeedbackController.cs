using cafe.Data;
using cafe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace cafe.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET /Feedback
        public IActionResult Index()
        {
            return View();
        }

        // POST /Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string content, int rating)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var feedback = new Feedback
            {
                Content = content,
                Rating = rating,
                CreatedDate = DateTime.Now,
                UserId = user?.Id ?? "Anonymous" // Allow anonymous feedback but link to user if logged in
            };

            _db.Feedbacks.Add(feedback);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã gửi phản hồi cho chúng tôi!";
            return RedirectToAction("Index");
        }
    }
}
