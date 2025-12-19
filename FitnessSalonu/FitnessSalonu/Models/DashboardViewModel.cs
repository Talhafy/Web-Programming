namespace FitnessSalonu.Models
{
    public class DashboardViewModel
    {
        // Üstteki Kartlar İçin Sayılar
        public int TotalUsers { get; set; }     // Toplam Üye
        public int TotalTrainers { get; set; }  // Toplam Antrenör
        public int TotalGyms { get; set; }      // Toplam Salon
        public int TotalAppointments { get; set; } // Toplam Randevu
    }
}