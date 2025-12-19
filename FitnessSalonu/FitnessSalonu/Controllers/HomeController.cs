using FitnessSalonu.Data;
using FitnessSalonu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FitnessSalonu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Giren kişi Admin mi?
            if (User.IsInRole("Admin"))
            {
                var model = new DashboardViewModel
                {
                    TotalUsers = await _userManager.Users.CountAsync(),
                    TotalTrainers = await _context.Trainers.CountAsync(),
                    TotalGyms = await _context.Gyms.CountAsync(),
                    TotalAppointments = await _context.Appointments.CountAsync()
                };

                return View(model);
            }

            // Normal kullanıcı
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}