namespace FitnessSalonu.Models
{
    public class DashboardViewModel
    {
        // Üstteki Kartlar İçin Sayılar
        public int TotalUsers { get; set; }     // Toplam Üye
        public int TotalTrainers { get; set; }  // Toplam Antrenör
        public int TotalGyms { get; set; }      // Toplam Salon
        public int TotalAppointments { get; set; } // Toplam Randevu

        // Grafikler İçin Veri Listeleri
        // 1. Grafik: Hangi hoca kaç randevu almış?
        public List<string> TrainerNames { get; set; } = new List<string>();
        public List<int> TrainerAppointmentCounts { get; set; } = new List<int>();

        // 2. Grafik: Hangi hizmet daha popüler?
        public List<string> ServiceNames { get; set; } = new List<string>();
        public List<int> ServiceCounts { get; set; } = new List<int>();
    }
}