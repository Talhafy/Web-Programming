using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessSalonu.Data;
using FitnessSalonu.Models;

namespace FitnessSalonu.Controllers
{
    public class GymServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GymServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GymServices
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.GymServices.Include(g => g.Gym);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: GymServices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymService = await _context.GymServices
                .Include(g => g.Gym)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gymService == null)
            {
                return NotFound();
            }

            return View(gymService);
        }

        // GET: GymServices/Create
        public IActionResult Create()
        {
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name");
            return View();
        }

        // POST: GymServices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,DurationMinutes,Price,GymId")] GymService gymService)
        {
            ModelState.Remove("Gym");
            if (ModelState.IsValid)
            {
                _context.Add(gymService);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", gymService.GymId);
            return View(gymService);
        }

        // GET: GymServices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymService = await _context.GymServices.FindAsync(id);
            if (gymService == null)
            {
                return NotFound();
            }
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", gymService.GymId);
            return View(gymService);
        }

        // POST: GymServices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DurationMinutes,Price,GymId")] GymService gymService)
        {
            if (id != gymService.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gymService);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GymServiceExists(gymService.Id))
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
            ViewData["GymId"] = new SelectList(_context.Gyms, "Id", "Name", gymService.GymId);
            return View(gymService);
        }

        // GET: GymServices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymService = await _context.GymServices
                .Include(g => g.Gym)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gymService == null)
            {
                return NotFound();
            }

            return View(gymService);
        }

        // POST: GymServices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gymService = await _context.GymServices.FindAsync(id);
            if (gymService != null)
            {
                _context.GymServices.Remove(gymService);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GymServiceExists(int id)
        {
            return _context.GymServices.Any(e => e.Id == id);
        }
    }
}
