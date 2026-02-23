using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Application.Sessions;
using TimeCapital.Data;
using TimeCapital.Data.Identity;
using TimeCapital.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor Pages (Identity UI)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (email/senha)
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Application Services
builder.Services.AddScoped<ISessionService, SessionService>();

// ✅ User context real (Identity)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, IdentityUserContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Identity UI endpoints
app.MapRazorPages();

// Controllers / MVC
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();