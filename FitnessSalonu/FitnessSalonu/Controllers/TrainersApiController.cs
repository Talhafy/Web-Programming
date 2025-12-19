using FitnessSalonu.Data;
using FitnessSalonu.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessSalonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TÜM ANTRENÖRLERİ GETİREN API
        // İstek Adresi: GET /api/TrainersApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            //GymService.Name ile ilişki var.
            var trainers = await _context.Trainers
                .Include(t => t.Gym)        // Salon bilgisi
                .Include(t => t.GymService) // Hizmet bilgisi
                .Select(t => new
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    // Eğer Hizmet atanmamışsa hata vermesin
                    Expertise = t.GymService != null ? t.GymService.Name : "Belirtilmemiş",
                    GymName = t.Gym != null ? t.Gym.Name : "Belirtilmemiş",
                    WorkingHours = t.WorkingHours
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // 2. FİLTRELEME YAPAN API (Uzmanlık Alanına Göre)
        // İstek Adresi: GET /api/TrainersApi/search?skill
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchTrainers(string skill)
        {
            if (string.IsNullOrEmpty(skill))
            {
                return BadRequest("Lütfen aranacak bir uzmanlık alanı girin. (Örn: ?skill=Pilates)");
            }

            // Girilen kelimeyi GymService tablosundaki Name alanında arıyoruz
            var result = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.GymService)
                .Where(t => t.GymService != null && t.GymService.Name.ToLower().Contains(skill.ToLower()))
                .Select(t => new
                {
                    t.FullName,
                    Expertise = t.GymService.Name,
                    GymName = t.Gym != null ? t.Gym.Name : "Belirtilmemiş"
                })
                .ToListAsync();

            if (!result.Any())
            {
                return NotFound("Bu uzmanlık alanında antrenör bulunamadı.");
            }

            return Ok(result);
        }
    }
}