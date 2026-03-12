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
            
            return View(new cafe.Models.ViewModels.LoginViewModel { });
        }

        // POST /UserAccount/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(cafe.Models.ViewModels.LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Admin không được đăng nhập qua form user
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                ModelState.AddModelError("", "Vui lòng dùng trang đăng nhập Admin.");
                return View(model);
            }

            var result = await _signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // GET /UserAccount/Register
        [HttpGet]
        public IActionResult Register() => View(new cafe.Models.ViewModels.RegisterViewModel());

        // POST /UserAccount/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(cafe.Models.ViewModels.RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email, 
                Email = model.Email,
                FullName = model.FullName, 
                EmailConfirmed = true
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
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
