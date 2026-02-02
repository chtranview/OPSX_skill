using Microsoft.EntityFrameworkCore;
using MyWeb.Models;

namespace MyWeb.Data;

public class TpiUbikeDbContext : DbContext
{
    public TpiUbikeDbContext(DbContextOptions<TpiUbikeDbContext> options) : base(options)
    {
    }

    public DbSet<TpiUbikeAreaRecord> TpiUbikeAreaRecords => Set<TpiUbikeAreaRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TpiUbikeAreaRecord>(entity =>
        {
            entity.Property(e => e.CollectedTime).HasColumnType("datetime2");
            entity.Property(e => e.Sno).IsRequired().HasMaxLength(32);
            entity.Property(e => e.Sna).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Snaen).HasMaxLength(200);
            entity.Property(e => e.Sarea).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Sareaen).HasMaxLength(100);
            entity.Property(e => e.Ar).HasMaxLength(300);
            entity.Property(e => e.Aren).HasMaxLength(300);
            entity.Property(e => e.Act).HasMaxLength(4);
            entity.Property(e => e.Mday).HasMaxLength(50);
            entity.Property(e => e.SrcUpdateTime).HasMaxLength(50);
            entity.Property(e => e.UpdateTime).HasMaxLength(50);
            entity.Property(e => e.InfoTime).HasMaxLength(50);
            entity.Property(e => e.InfoDate).HasMaxLength(50);
        });
    }
}
