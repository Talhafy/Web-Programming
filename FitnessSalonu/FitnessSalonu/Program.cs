using FitnessSalonu.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// 🔴 EKLENEN KISIM: PostgreSQL Tarih Hatası Çözümü
// Bu satır 'var builder' satırından ÖNCE gelmek zorundadır.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// =====================
// DATABASE (PostgreSQL)
// =====================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================
// IDENTITY + ROLES + UI
// =====================
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Geliştirme ortamı için e-posta onayını kapatıyoruz
        options.SignIn.RequireConfirmedAccount = false;

        // Admin şifresi "sau" olmalı.
        options.Password.RequiredLength = 3;       // En az 3 karakter ("sau" için)
        options.Password.RequireDigit = false;     // Rakam zorunlu değil
        options.Password.RequireLowercase = false; // Küçük harf zorunlu değil
        options.Password.RequireUppercase = false; // Büyük harf zorunlu değil
        options.Password.RequireNonAlphanumeric = false; // Sembol zorunlu değil
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// =====================
// MVC + RAZOR PAGES
// =====================
builder.Services.AddControllersWithViews();
// ============================================================
// 🔴 GEMINI İÇİN HTTP CLIENT AYARI (Hatanın Çözümü)
// ============================================================
builder.Services.AddHttpClient("GeminiClient", client =>
{
    // Adresin kökünü buraya sabitliyoruz. Hata şansı kalmıyor.
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(30); // 30 saniye cevap bekleme süresi
});
// ============================================================
builder.Services.AddRazorPages();

var app = builder.Build();

// =====================
// ERROR HANDLING
// =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// =====================
// AUTH (Kimlik Doğrulama)
// =====================
app.UseAuthentication();
app.UseAuthorization();

// =====================
// ROUTING
// =====================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Login/Register sayfaları için gerekli

// =====================
// SEED DATA (Admin ve Rol Oluşturma)
// =====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // Veritabanı yoksa oluşturur ve admin'i ekler
    await DbInitializer.SeedRolesAndAdminAsync(services);
}

app.Run();