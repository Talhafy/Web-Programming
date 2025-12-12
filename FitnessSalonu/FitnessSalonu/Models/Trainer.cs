using System.ComponentModel.DataAnnotations;

namespace FitnessSalonu.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Expertise { get; set; } // Kas, kilo verme

        public int GymId { get; set; }
        public Gym Gym { get; set; }
    }
}
