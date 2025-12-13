using FitnessSalonu.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        // 🔴 PROJE İSTERİ: Admin şifresi "sau" olmalı.
        // Bu yüzden şifre kurallarını gevşetiyoruz:
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