using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories;
using ASP_MessageBoard.Repositories.Implementations;
using ASP_MessageBoard.Repositories.Interfaces;
using ASP_MessageBoard.Services.Implementations;
using ASP_MessageBoard.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>(); // sql連接工廠
builder.Services.AddScoped<IUserRepository, UserRepository>(); // 使用者資料存取服務
builder.Services.AddScoped<IAccountService, AccountService>(); // 帳號服務
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>(); // 密碼雜湊服務
builder.Services.AddScoped<IPostRepository, PostRepository>(); // 文章資料存取服務
builder.Services.AddSingleton<IImageStorageService, LocalImageStorageService>(); // 文章圖片儲存服務
builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "ASP_MessageBoard.Auth";
        options.Cookie.HttpOnly = true; // JavaScript 無法讀取登入 Cookie，降低 Cookie 被 XSS 竊取的風險。
        options.Cookie.SameSite = SameSiteMode.Lax; // 降低跨站請求攜帶 Cookie 的風險。
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS 時只透過 HTTPS 傳送，本機 HTTP 開發仍可運作。

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true; // 使用者持續操作時延長登入有效期。
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
