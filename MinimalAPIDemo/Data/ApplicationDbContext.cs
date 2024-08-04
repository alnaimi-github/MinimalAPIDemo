using Microsoft.EntityFrameworkCore;
using MinimalAPIDemo.Models;

namespace MinimalAPIDemo.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Coupon>().HasData(new List<Coupon>
        {
            new() { Id = 1, Name = "10OF", Percent = 10, IsActive = true },
            new() { Id = 2, Name = "20OF", Percent = 20, IsActive = true }
        });
    }
}