using Microsoft.EntityFrameworkCore;
using DevFlow.Models;

namespace DevFlow.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Session> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Project entity
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.HasIndex(p => p.Name);
            
            // Configure relationship
            entity.HasMany(p => p.Sessions)
                .WithOne(s => s.Project)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Session entity
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.ProjectId).IsRequired();
            entity.Property(s => s.StartTime).IsRequired();
            entity.Property(s => s.IsActive).IsRequired();
            
            // Create indexes for common queries
            entity.HasIndex(s => s.ProjectId);
            entity.HasIndex(s => s.IsActive);
            entity.HasIndex(s => s.StartTime);
            entity.HasIndex(s => new { s.ProjectId, s.IsActive });
        });
    }
}
