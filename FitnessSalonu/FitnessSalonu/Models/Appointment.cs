using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // ForeignKey için gerekli
using Microsoft.AspNetCore.Identity; // Kullanıcı sistemi için gerekli

namespace FitnessSalonu.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        // Kullanıcının sadece ID'si değil, kendisi de lazım
        [Required]
        public string UserId { get; set; }

        // 🔴 YENİ EKLENEN KISIM: Kullanıcı Nesnesi
        // Bu sayede "randevu.User.UserName" diyip ismini çekebileceğiz.
        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        // Eğitmen İlişkisi
        public int TrainerId { get; set; }
        public virtual Trainer? Trainer { get; set; }

        // Hizmet İlişkisi
        public int GymServiceId { get; set; }
        public virtual GymService? GymService { get; set; }

        public DateTime AppointmentDate { get; set; }

        public bool IsApproved { get; set; }
    }
}