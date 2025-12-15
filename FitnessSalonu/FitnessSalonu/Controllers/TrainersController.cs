using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessSalonu.Data;
using FitnessSalonu.Models;
using Microsoft.AspNetCore.Authorization;

namespace FitnessSalonu.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Adminler erişebilir
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trainers
        public async Task<IActionResult> Index()
        {
            // Hem Salonu hem de Hizmeti (Uzmanlığı) dahil ediyoruz
            var trainers = _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.GymService);
            return View(await trainers.ToListAsync());
        }

        // GET: Trainers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.GymService)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // GET: Trainers/Create
        public IActionResult Create()
        {
            // Sadece Salonları gönderiyoruz. Hizmetler, salon seçilince AJAX ile gelecek.
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name");
            return View();
        }

        // POST: Trainers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // DİKKAT: Expertise YOK, GymServiceId VAR. WorkingHours EKLENDİ.
        public async Task<IActionResult> Create([Bind("Id,FullName,WorkingHours,GymId,GymServiceId")] Trainer trainer)
        {
            // İlişkili tablo hatalarını yoksay (Validasyon için)
            ModelState.Remove("Gym");
            ModelState.Remove("GymService");

            if (ModelState.IsValid)
            {
                _context.Add(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata olursa listeyi tekrar doldur
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            return View(trainer);
        }

        // GET: Trainers/Edit/5
        // GET: Trainers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null) return NotFound();

            // Sadece Salon listesini gönderiyoruz.
            // Hizmet listesini JavaScript (AJAX) ile sayfa açılınca dolduracağız.
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);

            return View(trainer);
        }

        // POST: Trainers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,WorkingHours,GymId,GymServiceId")] Trainer trainer)
        {
            if (id != trainer.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Gym");
            ModelState.Remove("GymService");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", trainer.GymId);
            return View(trainer);
        }

        // GET: Trainers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.GymService)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // POST: Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }

        // ============================================================
        // 🔴 AJAX API (BU KISIM EKLENDİ)
        // ============================================================
        [HttpGet]
        public JsonResult GetServicesByGym(int gymId)
        {
            // Seçilen salona ait hizmetleri JSON olarak döndürür
            var services = _context.GymServices
                .Where(s => s.GymId == gymId)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();
            return Json(services);
        }
    }
}