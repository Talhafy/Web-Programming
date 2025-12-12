using System;
using System.ComponentModel.DataAnnotations;

namespace FitnessSalonu.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Identity User

        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        public int GymServiceId { get; set; }
        public GymService GymService { get; set; }

        public DateTime AppointmentDate { get; set; }

        public bool IsApproved { get; set; }
    }
}
