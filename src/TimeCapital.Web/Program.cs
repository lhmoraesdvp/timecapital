using Microsoft.EntityFrameworkCore;
using TimeCapital.Application.Sessions;
using TimeCapital.Data;
using TimeCapital.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext (ajuste o nome da connection string se o seu for diferente)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application Services
builder.Services.AddScoped<ISessionService, SessionService>();

// Dev user (sem Identity por enquanto)
builder.Services.AddScoped<IUserContext, DevUserContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// sem auth por enquanto
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
