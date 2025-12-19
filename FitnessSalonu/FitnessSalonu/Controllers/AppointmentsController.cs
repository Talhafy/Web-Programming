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
                    .ThenInclude(gs => gs.Gym) 
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

        // 1. CREATE (GET) METODU
        public IActionResult Create()
        {

            ViewData["Gyms"] = _context.Gyms.ToList();
            ViewData["GymServiceId"] = new SelectList(new List<string>());
            ViewData["TrainerId"] = new SelectList(new List<string>());

            return View();
        }

        // 2. CREATE (POST) METODU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TrainerId,GymServiceId,AppointmentDate")] Appointment appointment)
        {

            // Güvenlik Kontrolü
            if (appointment.TrainerId == 0)
            {
                ModelState.AddModelError("TrainerId", "Lütfen geçerli bir antrenör seçiniz.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.UserId = userId;

            // Şimdilik standart "Beklemede" kalsın, admin listeden onaylar.
            appointment.Status = "Beklemede";

            ModelState.Remove("User");
            ModelState.Remove("Trainer");
            ModelState.Remove("GymService");
            ModelState.Remove("UserId");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();

                // Admin eklediyse -> Admin Paneline dön
                // Üye eklediyse -> Randevularım sayfasına dön
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction(nameof(AdminIndex));
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

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

        // API

        // 1. Salon seçilince Hizmetleri getirir
        [HttpGet]
        public JsonResult GetServicesByGym(int gymId)
        {
            var services = _context.GymServices
                .Where(s => s.GymId == gymId)
                .Select(s => new {
                    id = s.Id,
                    name = s.Name,
                    price = s.Price,
                    durationMinutes = s.DurationMinutes 
                })
                .ToList();
            return Json(services);
        }

        // Hizmet seçilince Hocaları getirir
        [HttpGet]
        public JsonResult GetTrainersByService(int gymId, int serviceId)
        {
            //GymServiceId ile eşleşen hocalar
            var trainers = _context.Trainers
                .Where(t => t.GymId == gymId && t.GymServiceId == serviceId)
                .Select(t => new { id = t.Id, fullName = t.FullName })
                .ToList();
            return Json(trainers);
        }

        //SAAT DİLİMLERİNİ HESAPLAYAN AKILLI MOTOR

        [HttpGet]
        public async Task<JsonResult> GetAvailableSlots(int trainerId, int serviceId, string date)
        {
            // 1. Gerekli Verileri Çek
            var trainer = await _context.Trainers.Include(t => t.Gym).FirstOrDefaultAsync(t => t.Id == trainerId);
            var service = await _context.GymServices.FindAsync(serviceId);

            if (trainer == null || service == null || string.IsNullOrEmpty(date))
                return Json(new List<string>());

            DateTime selectedDate = DateTime.Parse(date);

            //Salonun Açılış/Kapanış Saatlerini Çek
            TimeSpan openTime = TimeSpan.Parse(trainer.Gym.OpeningTime); 
            TimeSpan closeTime = TimeSpan.Parse(trainer.Gym.ClosingTime); 
            int duration = service.DurationMinutes;

            // Hoca İçin Alınmış Randevuları Bul
            var existingAppointments = await _context.Appointments
                .Where(a => a.TrainerId == trainerId && a.AppointmentDate.Date == selectedDate.Date)
                .Select(a => a.AppointmentDate.TimeOfDay) // Sadece saat kısmı
                .ToListAsync();

            // Slotları Oluşturma
            List<string> availableSlots = new List<string>();
            TimeSpan currentSlot = openTime;

            // Salon kapanana kadar döngü kur
            while (currentSlot.Add(TimeSpan.FromMinutes(duration)) <= closeTime)
            {
                // ÇAKIŞMA KONTROLÜ:
                // Eğer oluşturduğumuz bu saat diliminde veritabanında kayıt varsa, listeye ekleme.
                bool isTaken = false;

                //Eğer o saatte tam başlayan bir randevu varsa doludur.
 
                foreach (var appointmentTime in existingAppointments)
                {
                    // Eğer randevu saati ile şu anki slot aynıysa veya çakışıyorsa
                    if (appointmentTime == currentSlot)
                    {
                        isTaken = true;
                        break;
                    }
                }

                if (!isTaken)
                {
                    // Saat formatı
                    availableSlots.Add(currentSlot.ToString(@"hh\:mm"));
                }

                // Bir sonraki seansa geç (Hizmet süresi kadar ekle)
                currentSlot = currentSlot.Add(TimeSpan.FromMinutes(duration));
            }

            return Json(availableSlots);
        }
    }
}