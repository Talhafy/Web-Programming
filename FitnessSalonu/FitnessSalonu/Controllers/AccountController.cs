using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitnessSalonu.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // ÇIKIŞ YAPMA İŞLEMİ (Burada biz yönetiyoruz)
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // Sistemden at
            return RedirectToAction("Index", "Home"); // Ana sayfaya gönder
        }
    }
}