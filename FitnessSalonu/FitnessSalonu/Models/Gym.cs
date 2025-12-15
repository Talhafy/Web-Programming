using System.ComponentModel.DataAnnotations;

namespace FitnessSalonu.Models
{
    public class Gym
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        // YENİ: Açılış ve Kapanış Saatleri (String olarak tutmak en kolayıdır: "09:00")
        [Required]
        public string OpeningTime { get; set; } = "09:00";

        [Required]
        public string ClosingTime { get; set; } = "22:00";

        // İLİŞKİLER
        public virtual ICollection<Trainer>? Trainers { get; set; }
        public virtual ICollection<GymService>? GymServices { get; set; }
    }
}