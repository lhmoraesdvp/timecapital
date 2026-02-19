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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Area>(entity =>
        {
            entity.ToTable("Areas");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(x => x.NormalizedName)
                .HasMaxLength(60)
                .IsRequired();

            entity.Property(x => x.Color)
                .HasMaxLength(30);

            entity.HasIndex(x => new { x.UserId, x.NormalizedName })
                .IsUnique();
        });

        // (opcional agora) mapear Session/Goal depois, quando formos implementar
    }
}
