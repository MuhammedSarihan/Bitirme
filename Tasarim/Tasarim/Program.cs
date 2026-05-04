
using LlmService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tasarim.Data;
using Tasarim.Service.Abstract;
using Tasarim.Service.Concrate;
using Tasarim.Service.Concrete;
using Tasarim.Service.Concrete.LLM;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// HttpClient'ı sisteme tanıtıyoruz (Ollama ve Groq'taki _http kullanımı için)
builder.Services.AddHttpClient();

// Hangi LLM'i kullanmak istiyorsan onun başındaki yorum satırını kaldır. 

//YEREL
//builder.Services.AddScoped<ILlmProvider, OllamaLlmProvider>();

//GROQ API
builder.Services.AddScoped<ILlmProvider, GroqLlmProvider>();

// LLM Yöneticilerini sisteme kaydediyoruz
builder.Services.AddScoped<YorumAnalizYoneticisi>();
builder.Services.AddScoped<KumelemeYoneticisi>();

// Gemini SERVİS KAYITLARI BURAYA 
builder.Services.AddScoped<IGeminiProvider, GeminiLlmProvider>();
builder.Services.AddScoped<UrunGorselYoneticisi>();



// sepet
builder.Services.AddScoped<ISepetService, SepetService>();


builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Site.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.IOTimeout = TimeSpan.FromMinutes(10);
});

// Veritabanı bağlantısı (appsettings.json'daki "TasarimDbConnection" bağlantı adresini kullanarak)
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TasarimDbConnection")));

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
app.UseSession();// Oturum açmadan favoriler kullanmak için session kullanacağız

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
