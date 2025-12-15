using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessSalonu.Data;
using FitnessSalonu.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FitnessSalonu.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // KULLANICI İÇİN RANDEVULARIM
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointments = _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.GymService)
                    .ThenInclude(gs => gs.Gym) // Salon bilgisini de çek
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate);

            return View(await appointments.ToListAsync());
        }

        // YÖNETİCİ İÇİN TÜM RANDEVULAR
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var appointments = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.GymService)
                .OrderByDescending(a => a.AppointmentDate);
            return View(await appointments.ToListAsync());
        }

        // DURUM DEĞİŞTİRME (ONAYLA / REDDET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(AdminIndex));
        }

        // ==========================================
        // 🔴 SORUNU ÇÖZEN KISIM BURASI (GET Create)
        // ==========================================
        public IActionResult Create()
        {
            // Admin yanlışlıkla buraya girerse paneline gönder
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("AdminIndex");
            }

            // 1. ADIM: Tüm Salonları Çekip Sayfaya Gönderiyoruz!
            // Bu satır olmazsa ilk kutu BOŞ gelir.
            ViewData["Gyms"] = _context.Gyms.ToList();

            // Diğer kutular (Hizmet ve Hoca) salon seçilince AJAX ile dolacak.
            // O yüzden şimdilik boş gönderiyoruz.
            ViewData["GymServiceId"] = new SelectList(new List<string>());
            ViewData["TrainerId"] = new SelectList(new List<string>());

            return View();
        }

        // RANDEVU KAYDETME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TrainerId,GymServiceId,AppointmentDate")] Appointment appointment)
        {
            if (User.IsInRole("Admin")) return RedirectToAction("AdminIndex");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.UserId = userId;

            // Otomatik Durum: Beklemede
            appointment.Status = "Beklemede";

            // Validasyon temizliği
            ModelState.Remove("User");
            ModelState.Remove("Trainer");
            ModelState.Remove("GymService");
            ModelState.Remove("UserId");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata olursa salonları tekrar yükle ki sayfa bozulmasın
            ViewData["Gyms"] = _context.Gyms.ToList();
            return View(appointment);
        }

        // SİLME EKRANI (Admin İçin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.GymService)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null) _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminIndex));
        }

        // ==========================================
        // 🔴 AJAX API (Kutuların Dolması İçin Şart)
        // ==========================================

        // 1. Salon seçilince Hizmetleri getirir
        [HttpGet]
        public JsonResult GetServicesByGym(int gymId)
        {
            var services = _context.GymServices
                .Where(s => s.GymId == gymId)
                .Select(s => new { id = s.Id, name = s.Name, price = s.Price })
                .ToList();
            return Json(services);
        }

        // 2. Hizmet seçilince Hocaları getirir
        [HttpGet]
        public JsonResult GetTrainersByService(int gymId, int serviceId)
        {
            // YENİ SİSTEM: GymServiceId ile eşleşen hocalar
            var trainers = _context.Trainers
                .Where(t => t.GymId == gymId && t.GymServiceId == serviceId)
                .Select(t => new { id = t.Id, fullName = t.FullName })
                .ToList();
            return Json(trainers);
        }
    }
}