// ============================================================================
// Features/Auth/Data/AuthDbContext.cs - FRISSÍTETT (kibővített User mezőkkel)
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
                // Email egyedi index
                entity.HasIndex(e => e.Email).IsUnique();

                // Cím mezők konfigurálása
                entity.Property(u => u.Iranyitoszam).HasMaxLength(4);
                entity.Property(u => u.Telepules).HasMaxLength(100);
                entity.Property(u => u.Utca).HasMaxLength(150);
                entity.Property(u => u.Hazszam).HasMaxLength(20);
                entity.Property(u => u.Cim).HasMaxLength(300);
            });

            // Seed: Admin user
            SeedAdminUser(modelBuilder);
        }

        private static void SeedAdminUser(ModelBuilder modelBuilder)
        {
            // Admin jelszó: "Admin123"
            string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@szerszam.hu",
                    PasswordHash = adminPasswordHash,
                    Role = "Admin",
                    // Admin-nak nem kötelezőek a cím mezők
                    Iranyitoszam = "1011",
                    Telepules = "Budapest",
                    Utca = "Fő utca",
                    Hazszam = "1",
                    Telefonszam = null,
                    Cim = "1011 Budapest, Fő utca 1",
                    Nev = "Admin",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}