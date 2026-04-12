using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db.Entities;

namespace UTB.Minute.Db;

public class MinuteDbContext : DbContext
{
    public MinuteDbContext(DbContextOptions<MinuteDbContext> options)
        : base(options)
    {
    }

    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Meal>()
            .Property(m => m.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Meal>()
            .Property(m => m.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Meal>()
            .Property(m => m.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<MenuItem>()
            .HasOne(m => m.Meal)
            .WithMany(m => m.MenuItems)
            .HasForeignKey(m => m.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItem>()
            .Property(m => m.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<MenuItem>()
            .Property(m => m.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Order>()
            .HasOne(o => o.MenuItem)
            .WithMany(m => m.Orders)
            .HasForeignKey(o => o.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .Property(o => o.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Order>()
            .Property(o => o.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

    }
}
