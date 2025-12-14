using FitnessSalonu.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Google.GenAI;
using Google.GenAI.Types;

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
        public async Task<IActionResult> GeneratePlan(AIRequest request)
        {
            // 1. BMI Hesapla
            double heightInMeters = request.Height / 100;
            double bmi = request.Weight / (heightInMeters * heightInMeters);
            string bmiStatus = bmi < 18.5 ? "Zayıf" : bmi < 25 ? "Normal" : bmi < 30 ? "Fazla Kilolu" : "Obezite Riski";

            // 2. Prompt (Emir) Hazırla
            string prompt = $@"
                Sen uzman bir spor koçusun (Model: Gemini 2.5).
                Kullanıcı: {request.Gender}, {request.Weight}kg, {request.Height}cm.
                Durum: {bmiStatus}.
                Hedef: {request.Goal}.

                Lütfen cevabını SADECE şu JSON formatında ver (Markdown yok, ```json yazma):
                {{
                    ""DietPlan"": ""(Buraya samimi bir dille, emojiler kullanarak detaylı bir diyet tavsiyesi yaz)"",
                    ""WorkoutPlan"": ""(Buraya emojilerle süslenmiş, detaylı bir antrenman programı yaz)""
                }}
            ";

            try
            {
                // 🔴 3. ANAHTARINI BURAYA YAPIŞTIR
                string apiKey = "AIzaSyCQUz_xKGTi8FDyN6aUHUqTH8P-uWNlWD4";

                // 🔴 4. RESMİ KODUN ENTEGRASYONU
                // Dokümanda 'var client = new Client();' diyordu ama o environment variable arar.
                // Biz anahtarı elle veriyoruz:
                var client = new Client(apiKey: apiKey);

                // Dokümandaki gibi 'GenerateContentAsync' kullanıyoruz
                // Model ismini senin resimdeki gibi 'gemini-2.5-flash' yaptık.
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.5-flash",
                    contents: prompt
                );

                // 🔴 5. CEVABI ALMA
                // Dokümandaki: response.Candidates[0].Content.Parts[0].Text
                string aiText = response.Candidates[0].Content.Parts[0].Text;

                if (string.IsNullOrEmpty(aiText))
                {
                    return Content("Yapay zeka boş cevap döndürdü.");
                }

                // Temizlik
                aiText = aiText.Replace("```json", "").Replace("```", "").Trim();

                var aiPlan = JsonConvert.DeserializeObject<AIResponse>(aiText);
                aiPlan.BMI = Math.Round(bmi, 2);
                aiPlan.BMISTatus = bmiStatus;

                return View("Result", aiPlan);
            }
            catch (Exception ex)
            {
                return Content($"Hata Oluştu: {ex.Message}\n\nİpucu: Eğer paket hatası alıyorsan NuGet'ten 'Google.GenAI' paketini yüklediğine emin ol.");
            }
        }
    }
}