using BackgroundWorker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace BackgroundWorker.Data;

public class BackgroundWorkerDbContext(DbContextOptions<BackgroundWorkerDbContext> options) : OutboxDbContext(options)
{
    public DbSet<Document> Documents { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.ExtractedText).HasColumnType("text");
        });
    }
}