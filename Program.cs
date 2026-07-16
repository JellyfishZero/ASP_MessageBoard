using ASP_MessageBoard.Models.Entities;
using ASP_MessageBoard.Repositories;
using ASP_MessageBoard.Repositories.Implementations;
using ASP_MessageBoard.Repositories.Interfaces;
using ASP_MessageBoard.Services.Implementations;
using ASP_MessageBoard.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>(); // sql連接工廠
builder.Services.AddScoped<IUserRepository, UserRepository>(); // 使用者資料存取服務
builder.Services.AddScoped<IAccountService, AccountService>(); // 帳號服務
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>(); // 密碼雜湊服務

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

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
