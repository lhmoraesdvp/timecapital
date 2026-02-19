using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Domain.Entities;
using TimeCapital.Data.Identity;

namespace TimeCapital.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Session> Sessions => Set<Session>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Project
        b.Entity<Project>(e =>
        {
            e.ToTable("Projects");
            e.HasKey(x => x.Id);

            e.Property(x => x.Title).HasMaxLength(120).IsRequired();
            e.Property(x => x.Status).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();

            e.HasIndex(x => new { x.OwnerId, x.Status });
            e.HasIndex(x => new { x.OwnerId, x.Title });

            e.HasOne<ApplicationUser>()
             .WithMany()
             .HasForeignKey(x => x.OwnerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Goal
        b.Entity<Goal>(e =>
        {
            e.ToTable("Goals");
            e.HasKey(x => x.Id);

            e.Property(x => x.TargetMinutes).IsRequired();
            e.HasIndex(x => x.ProjectId);

            e.HasOne(x => x.Project)
             .WithMany(p => p.Goals)
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Session
        b.Entity<Session>(e =>
        {
            e.ToTable("Sessions");
            e.HasKey(x => x.Id);

            e.Property(x => x.StartTimeUtc).IsRequired();
            e.Property(x => x.EndTimeUtc);
            e.Property(x => x.CanceledAtUtc);

            e.HasIndex(x => x.ProjectId);
            e.HasIndex(x => x.GoalId);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => new { x.UserId, x.StartTimeUtc });

            e.HasOne(x => x.Project)
             .WithMany(p => p.Sessions)
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Goal)
             .WithMany()
             .HasForeignKey(x => x.GoalId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne<ApplicationUser>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // ✅ 1 sessão ativa por usuário
            e.HasIndex(x => x.UserId)
             .IsUnique()
             .HasFilter("[EndTimeUtc] IS NULL AND [CanceledAtUtc] IS NULL");
        });

        // AspNetUsers: DefaultProjectId (índice opcional)
        b.Entity<ApplicationUser>()
         .HasIndex(u => u.DefaultProjectId);
    }
}
