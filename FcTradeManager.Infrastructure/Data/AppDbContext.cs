using Microsoft.EntityFrameworkCore;
using FcTradeManager.Domain.Entities;
using EAFC27.Infrastructure.Data;

namespace FcTradeManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Card> Cards => Set<Card>();
    public DbSet<User> Users => Set<User>();
    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();
    public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique();

        // RNF05
        modelBuilder.Entity<PriceSnapshot>()
            .HasIndex(p => new { p.CardId, p.RecordedAt });

        modelBuilder.Entity<WatchlistItem>()
            .HasIndex(w => new { w.UserId, w.CardId })
            .IsUnique();

        modelBuilder.Entity<Card>()
            .Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Trade>().Property(t => t.Type)
            .HasLowercaseStringConversion();

        modelBuilder.Entity<Alert>().Property(a => a.Kind)
            .HasLowercaseStringConversion();

        modelBuilder.Entity<PriceSnapshot>().Property(p => p.Source)
            .HasLowercaseStringConversion();
    }
}