using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Session;
using Veritabani_Odevi.DAL.Utilities;
using Veritabani_Odevi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session desteği ekle
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi
    options.Cookie.HttpOnly = true; // XSS koruması
    options.Cookie.IsEssential = true; // Temel çerez
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddSingleton<DatabaseConnection>();
builder.Services.AddScoped<KullaniciRepository>();
builder.Services.AddScoped<KitapRepository>();
builder.Services.AddScoped<KategoriRepository>();
builder.Services.AddScoped<KullaniciRaporRepository>();
builder.Services.AddScoped<YorumRepository>();
builder.Services.AddScoped<RaporRepository>();
builder.Services.AddScoped<SiparisRepository>();
builder.Services.AddScoped<SepetRepository>();
builder.Services.AddScoped<OdemeRepository>();
builder.Services.AddScoped<IndirimRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware'� UseRouting'den sonra, UseAuthorization'dan �nce gelmelidir
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();