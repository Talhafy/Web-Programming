using System.ComponentModel.DataAnnotations;

namespace FitnessSalonu.Models
{
    public class Gym
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(100)]
        public string WorkingHours { get; set; } // 08:00 - 22:00
    }
}
