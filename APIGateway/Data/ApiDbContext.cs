using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using Unadesk_Test.Data.Models;

namespace Unadesk_Test.Data;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : OutboxDbContext(options)
{
    public DbSet<DocumentMetadata> Documents { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); 

        modelBuilder.Entity<DocumentMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.ExtractedText).HasColumnType("text");
        });
    }
}