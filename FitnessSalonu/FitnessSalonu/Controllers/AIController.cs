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
            double heightInMeters = request.Height / 100;
            double bmi = request.Weight / (heightInMeters * heightInMeters);
            string bmiStatus = bmi < 18.5 ? "Zayıf" : bmi < 25 ? "Normal" : bmi < 30 ? "Fazla Kilolu" : "Obezite Riski";

            // 2. Prompt Hazırlama
            string prompt = $@"
                Sen uzman bir spor koçusun (Model: Gemini 2.5 Flash).
                Kullanıcı: {request.Gender}, {request.Weight}kg, {request.Height}cm.
                Durum: {bmiStatus}.
                Hedef: {request.Goal}.

                GÖREVİN:
                Kullanıcı için çok motive edici bir diyet ve antrenman programı hazırla.
                
                ÖNEMLİ KURAL (ANTRENMAN İÇİN):
                Antrenman programında yazdığın HER hareketin altına, o hareketin görselini getirecek şu HTML kodunu ekle:
                <br><img src='https://tse4.mm.bing.net/th?q=HAREKET_ISMI_BURAYA+gym+workout&w=200&h=200&c=7&rs=1' style='width:100%; max-width:200px; border-radius:10px; margin-top:5px; margin-bottom:15px;'><br>
                
                (Örnek: Eğer 'Bench Press' yazdıysan hemen altına <img src='https://tse4.mm.bing.net/th?q=Bench+Press+gym+workout...'> kodunu koymalısın.)

                Lütfen cevabını SADECE şu JSON formatında ver (Markdown yok, ```json yazma, sadece saf JSON):
                {{
                    ""DietPlan"": ""(Buraya emojilerle süslenmiş, samimi, maddeler halinde detaylı diyet listesi)"",
                    ""WorkoutPlan"": ""(Buraya hareket isimleri, set/tekrar sayıları ve yukarıda bahsettiğim HTML resim etiketlerini içeren detaylı program)""
                }}
            ";
            try
            {
                string apiKey = "AIzaSyCQUz_xKGTi8FDyN6aUHUqTH8P-uWNlWD4";

                var client = new Client(apiKey: apiKey);

                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.5-flash",
                    contents: prompt
                );

                // 🔴 5. CEVABI ALMA
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