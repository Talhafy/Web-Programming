using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessSalonu.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        public string? FullName { get; set; } 

        public int WorkingHours { get; set; } = 8;

        // İLİŞKİLER
        public int GymId { get; set; }
        public virtual Gym? Gym { get; set; }

        public int GymServiceId { get; set; }

        [ForeignKey("GymServiceId")]
        public virtual GymService? GymService { get; set; }
    }
}