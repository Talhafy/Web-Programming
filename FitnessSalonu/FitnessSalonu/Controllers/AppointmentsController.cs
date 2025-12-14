using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessSalonu.Data;
using FitnessSalonu.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FitnessSalonu.Controllers
{
    [Authorize] // Sadece giriş yapanlar görebilir
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. RANDEVULARIM SAYFASI
        public async Task<IActionResult> Index()
        {
            // Giriş yapan kullanıcının ID'sini bul
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sadece bu kullanıcıya ait randevuları getir
            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.GymService)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // 2. RANDEVU ALMA FORMU
        public IActionResult Create()
        {
            // Dropdown (Açılır Kutu) için verileri hazırla
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName");
            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "Id", "Name");
            return View();
        }

        // 3. KAYDETME VE ÇAKIŞMA KONTROLÜ (KRİTİK KISIM)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrainerId,GymServiceId,AppointmentDate")] Appointment appointment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.UserId = userId;
            appointment.IsApproved = true; // Otomatik onay

            // Seçilen hizmetin süresini bul (Bitiş saatini hesaplamak için)
            var service = await _context.GymServices.FindAsync(appointment.GymServiceId);
            if (service != null)
            {
                // Randevu Bitiş Saati = Başlangıç + Hizmet Süresi
                DateTime endTime = appointment.AppointmentDate.AddMinutes(service.DurationMinutes);

                // 🔴 ÇAKIŞMA KONTROLÜ 🔴
                // Seçilen hocanın, bu saat aralığında başka randevusu var mı?
                bool cakismaVarMi = await _context.Appointments
                    .AnyAsync(a => a.TrainerId == appointment.TrainerId &&
                                   a.AppointmentDate < endTime &&
                                   a.AppointmentDate.AddMinutes(a.GymService.DurationMinutes) > appointment.AppointmentDate);

                if (cakismaVarMi)
                {
                    ModelState.AddModelError("", "⚠️ Bu saatte antrenör dolu! Lütfen başka bir saat seçiniz.");
                }
            }

            // Validasyon hatalarını temizle (UserId vb. formdan gelmediği için)
            ModelState.Remove("Trainer");
            ModelState.Remove("GymService");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Listeye dön
            }

            // Hata varsa formu tekrar doldur
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "Id", "Name", appointment.GymServiceId);
            return View(appointment);
        }
    }
}