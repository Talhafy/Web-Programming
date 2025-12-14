using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Kullanıcı ID'sini bulmak için şart
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessSalonu.Data;
using FitnessSalonu.Models;
using Microsoft.AspNetCore.Authorization;

namespace FitnessSalonu.Controllers
{
    [Authorize] // KİLİT: Giriş yapmayan kimse bu sayfaları göremez!
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. KULLANICI İÇİN: Sadece Kendi Randevularını Görür
        // ============================================================
        public async Task<IActionResult> Index()
        {
            // Giriş yapan kullanıcının ID'sini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Veritabanından sadece BU KULLANICIYA ait randevuları getir
            var applicationDbContext = _context.Appointments
                .Include(a => a.GymService)
                .Include(a => a.Trainer)
                .Where(a => a.UserId == userId) // Filtreleme
                .OrderByDescending(a => a.AppointmentDate); // En yeni en üstte

            return View(await applicationDbContext.ToListAsync());
        }

        // ============================================================
        // 👑 2. ADMIN İÇİN: Tüm Randevuları Görür (Özel Panel)
        // ============================================================
        [Authorize(Roles = "Admin")] // Sadece Admin girebilir
        public async Task<IActionResult> AdminIndex()
        {
            var allAppointments = _context.Appointments
                .Include(a => a.GymService)
                .Include(a => a.Trainer)
                .Include(a => a.User) // Üye bilgisini de getir
                .OrderByDescending(a => a.AppointmentDate);

            return View(await allAppointments.ToListAsync());
        }

        // ============================================================
        // 3. RANDEVU ALMA SAYFASI (Formu Göster)
        // ============================================================
        public IActionResult Create()
        {
            // Açılır kutuları (Dropdown) doldur
            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "Id", "Name");
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName");
            return View();
        }

        // ============================================================
        // 4. RANDEVUYU KAYDET (Çakışma Kontrolü Burada!)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TrainerId,GymServiceId,AppointmentDate")] Appointment appointment)
        {
            // 1. Kullanıcı ID'sini otomatik ekle
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.UserId = userId;
            appointment.IsApproved = true; // Otomatik onaylı varsayalım

            // 2. Seçilen hizmetin süresini bul (Bitiş saatini hesaplamak için)
            var service = await _context.GymServices.FindAsync(appointment.GymServiceId);

            if (service != null)
            {
                // Başlangıç: Formdan gelen saat
                // Bitiş: Başlangıç + Hizmet Süresi (dk)
                DateTime startTime = appointment.AppointmentDate;
                DateTime endTime = startTime.AddMinutes(service.DurationMinutes);

                // 🔴 ÇAKIŞMA KONTROLÜ 🔴
                // Veritabanında, seçilen HOCANIN (TrainerId), o saat aralığında başka işi var mı?
                bool isBusy = await _context.Appointments
                    .AnyAsync(a => a.TrainerId == appointment.TrainerId &&
                                   a.AppointmentDate < endTime &&
                                   a.AppointmentDate.AddMinutes(a.GymService.DurationMinutes) > startTime);

                if (isBusy)
                {
                    // Hata Mesajı (Türkçe)
                    ModelState.AddModelError("", "⚠️ Üzgünüz, seçtiğiniz antrenör bu saat aralığında dolu. Lütfen başka bir saat veya antrenör seçiniz.");
                }
            }

            // 3. Validasyon hatalarını temizle (User, Trainer vb. nesneler formdan gelmediği için boş gelebilir, sorun yok)
            ModelState.Remove("User");
            ModelState.Remove("Trainer");
            ModelState.Remove("GymService");

            // 4. Hata yoksa KAYDET
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Listeye dön
            }

            // 5. Hata varsa formu tekrar doldurup göster
            ViewData["GymServiceId"] = new SelectList(_context.GymServices, "Id", "Name", appointment.GymServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            return View(appointment);
        }

        // ============================================================
        // 5. SİLME İŞLEMİ (Genellikle Admin veya Randevu Sahibi Yapar)
        // ============================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.GymService)
                .Include(a => a.Trainer)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            // Eğer silen kişi Admin ise Admin Listesine, değilse Kendi Listesine dönsün
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(AdminIndex));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}