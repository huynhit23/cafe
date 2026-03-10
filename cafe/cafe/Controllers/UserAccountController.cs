using cafe.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace cafe.Controllers
{
    public class UserAccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserAccountController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _signIn = signIn;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET /UserAccount/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST /UserAccount/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewData["Error"] = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // Admin không được đăng nhập qua form user
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                ViewData["Error"] = "Vui lòng dùng trang đăng nhập Admin.";
                return View();
            }

            var result = await _signIn.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            ViewData["Error"] = "Email hoặc mật khẩu không đúng.";
            return View();
        }

        // GET /UserAccount/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST /UserAccount/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewData["Error"] = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email, Email = email,
                FullName = fullName, EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                ViewData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
                return View();
            }

            await _userManager.AddToRoleAsync(user, "User");
            await _signIn.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        // POST /UserAccount/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();
    }
}
