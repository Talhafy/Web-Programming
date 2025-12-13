using FitnessSalonu.Data;
using FitnessSalonu.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessSalonu.Controllers
{
    // Bu etiket, buranın bir API olduğunu belirtir (Sayfa değil, veri döner)
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TÜM ANTRENÖRLERİ GETİREN API (LINQ Select Kullanımı)
        // İstek Adresi: GET /api/TrainersApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            // Veritabanından sadece gerekli bilgileri (Ad, Uzmanlık, Salon Adı) çekiyoruz.
            var trainers = await _context.Trainers
                .Include(t => t.Gym) // İlişkili Salon verisini dahil et
                .Select(t => new
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    Expertise = t.Expertise,
                    GymName = t.Gym.Name
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // 2. FİLTRELEME YAPAN API (LINQ Where Kullanımı)
        // İstek Adresi: GET /api/TrainersApi/search?skill=Yoga
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchTrainers(string skill)
        {
            if (string.IsNullOrEmpty(skill))
            {
                return BadRequest("Lütfen aranacak bir uzmanlık alanı girin. (Örn: ?skill=Pilates)");
            }

            // Girilen kelimeye göre filtreleme yapıyoruz
            var result = await _context.Trainers
                .Where(t => t.Expertise.ToLower().Contains(skill.ToLower()))
                .Select(t => new
                {
                    t.FullName,
                    t.Expertise,
                    GymName = t.Gym.Name
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