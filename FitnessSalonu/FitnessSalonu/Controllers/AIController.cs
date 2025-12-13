using FitnessSalonu.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitnessSalonu.Controllers
{
    public class AIController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GeneratePlan(AIRequest request)
        {
            // 1. BMI Hesapla
            double heightInMeters = request.Height / 100;
            double bmi = request.Weight / (heightInMeters * heightInMeters);

            var response = new AIResponse
            {
                BMI = Math.Round(bmi, 2)
            };

            // 2. Durumu Belirle
            if (bmi < 18.5) response.BMISTatus = "Zayıf";
            else if (bmi < 25) response.BMISTatus = "Normal Kilolu";
            else if (bmi < 30) response.BMISTatus = "Fazla Kilolu";
            else response.BMISTatus = "Obezite Riski";

            // 3. Basit Yapay Zeka Mantığı
            if (request.Goal == "Kilo Verme")
            {
                response.DietPlan = "Kalori açığı oluşturmalısın. Şeker ve beyaz ekmeği kes.";
                response.WorkoutPlan = "Haftada 4 gün Kardiyo + 2 gün Tüm Vücut Ağırlık.";
            }
            else if (request.Goal == "Kas Yapma")
            {
                response.DietPlan = "Yüksek protein almalısın (Kilo x 2 gr). Karbonhidratı artır.";
                response.WorkoutPlan = "Haftada 5 gün Bölgesel Antrenman (Push-Pull-Legs).";
            }
            else
            {
                response.DietPlan = "Dengeli ve düzenli beslen. İşlenmiş gıdadan kaçın.";
                response.WorkoutPlan = "Haftada 3 gün Pilates/Yoga + Yürüyüş.";
            }

            return View("Result", response);
        }
    }
}
