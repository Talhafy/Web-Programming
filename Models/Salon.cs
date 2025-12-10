using System.ComponentModel.DataAnnotations;

namespace FitnessCenter.Models
{
    public class Salon
    {
        public int Id { get; set; }

        [Required]
        public string Ad { get; set; }

        [Required]
        public string CalismaSaatleri { get; set; }

        public string Adres { get; set; }
    }
}
