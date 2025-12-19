namespace FitnessSalonu.Models
{
    // Kullanıcıdan alacağımız bilgiler
    public class AIRequest
    {
        public double Weight { get; set; } // Kilo
        public double Height { get; set; } // Boy
        public string Goal { get; set; }   // Hedef 
        public string Gender { get; set; } // Cinsiyet
    }

    // Kullanıcıya göstereceğimiz sonuçlar
    public class AIResponse
    {
        public double BMI { get; set; }       // Vücut Kitle İndeksi
        public string BMISTatus { get; set; } // Durum
        public string DietPlan { get; set; }  // Diyet
        public string WorkoutPlan { get; set; }// Antrenman
    }
}