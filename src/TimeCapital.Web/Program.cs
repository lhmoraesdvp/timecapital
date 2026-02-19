using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
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
app.MapGet("/_routes", (IEnumerable<EndpointDataSource> sources) =>
{
    var endpoints = sources
        .SelectMany(s => s.Endpoints)
        .Select(e => e.DisplayName);

    return Results.Ok(endpoints);
});

app.MapGet("/", () => Results.Redirect("/landing/index.html"));
app.MapControllers(); // ✅ ADICIONE ISTO

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
