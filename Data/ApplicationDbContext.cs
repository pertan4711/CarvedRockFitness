using Microsoft.EntityFrameworkCore;
using CarvedRockFitness.Models;

namespace CarvedRockFitness.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            // Configure CartItem entity
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AddedAt).HasDefaultValueSql("GETDATE()");
            });

            // Seed some sample data
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Hiking Boots",
                    Description = "Durable hiking boots for all terrains",
                    Category = "Footwear",
                    ImageUrl = "/images/products/boots/hiking-boots-1.jpg",
                    Price = 149.99m
                },
                new Product
                {
                    Id = 2,
                    Name = "Climbing Rope",
                    Description = "Professional grade climbing rope - 60m",
                    Category = "Climbing Gear",
                    ImageUrl = "/images/products/climbing gear/rope-1.jpg",
                    Price = 89.99m
                },
                new Product
                {
                    Id = 3,
                    Name = "Kayak Paddle",
                    Description = "Lightweight carbon fiber kayak paddle",
                    Category = "Water Sports",
                    ImageUrl = "/images/products/kayaks/paddle-1.jpg",
                    Price = 79.99m
                }
            );
        }
    }
}