using System.ComponentModel.DataAnnotations;

namespace FitnessSalonu.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Expertise { get; set; } // Uzmanlık (Kas, kilo verme vb.)

        // 🔴 YENİ EKLENEN KISIM: Çalışma Saati
        // Varsayılan olarak 8 saat atadık.
        public int WorkingHours { get; set; } = 8;

        public int GymId { get; set; }
        public virtual Gym? Gym { get; set; } // İlişki
    }
}