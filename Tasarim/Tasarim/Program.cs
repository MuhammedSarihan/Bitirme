
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Tasarim.Data;
using Tasarim.Service.Abstract;
using Tasarim.Service.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddScoped(typeof(IService<>), typeof(Service<>));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(x =>
{
    x.LoginPath = "/Hesap/SignIn";
    x.AccessDeniedPath = "/AccessDenied";
    x.Cookie.Name = "Hesap";
    x.Cookie.MaxAge = TimeSpan.FromDays(7);
    x.Cookie.IsEssential = true;
});
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("AdminPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
    x.AddPolicy("KullaniciPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Admin", "Musteri"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
 
app.MapStaticAssets();
app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Main}/{action=Index}/{id?}"
          );
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
