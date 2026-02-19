using Microsoft.EntityFrameworkCore;
using TimeCapital.Data;
using TimeCapital.Web.Security;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<Microsoft.AspNetCore.Identity.IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<ICurrentUser, MockCurrentUser>();
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
app.MapGet("/", () => Results.Redirect("/landing/index.html"));
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
