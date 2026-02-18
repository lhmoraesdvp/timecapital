using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Domain.Entities;

namespace TimeCapital.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Goal> Goals => Set<Goal>();
}
