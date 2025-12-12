using System.ComponentModel.DataAnnotations;

namespace FitnessSalonu.Models
{
    public class GymService
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Fitness, Yoga

        [Range(15, 180)]
        public int DurationMinutes { get; set; }

        [Range(0, 10000)]
        public decimal Price { get; set; }

        public int GymId { get; set; }
        public Gym Gym { get; set; }
    }
}
