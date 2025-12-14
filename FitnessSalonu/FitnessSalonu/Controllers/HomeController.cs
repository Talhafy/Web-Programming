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

        // Veritabanı ve Kullanıcı yöneticilerini içeri alıyoruz (Dependency Injection)
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 🔴 KRİTİK KONTROL: Giren kişi Admin mi?
            if (User.IsInRole("Admin"))
            {
                // Evet, Admin! O zaman istatistikleri hazırla.
                var model = new DashboardViewModel
                {
                    // 1. Temel Sayılar
                    TotalUsers = await _userManager.Users.CountAsync(),
                    TotalTrainers = await _context.Trainers.CountAsync(),
                    TotalGyms = await _context.Gyms.CountAsync(),
                    TotalAppointments = await _context.Appointments.CountAsync()
                };

                // 2. Grafik Verisi: En Popüler 5 Antrenör
                var trainerStats = await _context.Appointments
                    .Include(a => a.Trainer)
                    .GroupBy(a => a.Trainer.FullName)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToListAsync();

                model.TrainerNames = trainerStats.Select(x => x.Name).ToList();
                model.TrainerAppointmentCounts = trainerStats.Select(x => x.Count).ToList();

                // 3. Grafik Verisi: Popüler Hizmetler (Pasta Grafiği için)
                var serviceStats = await _context.Appointments
                    .Include(a => a.GymService)
                    .GroupBy(a => a.GymService.Name)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .ToListAsync();

                model.ServiceNames = serviceStats.Select(x => x.Name).ToList();
                model.ServiceCounts = serviceStats.Select(x => x.Count).ToList();

                // Admin paneli görünümüyle modeli gönder
                return View(model);
            }

            // Hayır, normal kullanıcı. Standart anasayfayı göster.
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