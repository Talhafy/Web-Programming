using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessSalonu.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public DateTime AppointmentDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // YENİ SÜTUN: Randevu Durumu (Beklemede, Onaylandı, Reddedildi)
        public string Status { get; set; } = "Beklemede";

        // İLİŞKİLER
        public string UserId { get; set; }
        public virtual Microsoft.AspNetCore.Identity.IdentityUser? User { get; set; }

        public int GymServiceId { get; set; }
        public virtual GymService? GymService { get; set; }

        public int TrainerId { get; set; }
        public virtual Trainer? Trainer { get; set; }
    }
}