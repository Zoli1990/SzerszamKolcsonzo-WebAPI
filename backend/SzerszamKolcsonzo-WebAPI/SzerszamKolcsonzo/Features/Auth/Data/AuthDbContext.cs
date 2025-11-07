// ============================================================================
// Features/Auth/Data/AuthDbContext.cs
// ============================================================================

using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Features.Auth.Models;

namespace SzerszamKolcsonzo.Features.Auth.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Seed: Admin user
            SeedAdminUser(modelBuilder);
        }

        private static void SeedAdminUser(ModelBuilder modelBuilder)
        {
            // Password: Admin123!
            // BCrypt hash
            string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@szerszam.hu",
                    PasswordHash = adminPasswordHash,
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}

