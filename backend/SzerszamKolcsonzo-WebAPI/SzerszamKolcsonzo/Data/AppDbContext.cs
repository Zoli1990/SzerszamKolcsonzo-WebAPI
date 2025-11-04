using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Models;

namespace SzerszamKolcsonzo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Kategoria> Kategoriak { get; set; }
        public DbSet<Eszkoz> Eszkozok { get; set; }
        public DbSet<Foglalas> Foglalasok { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Kategoria konfiguráció
            modelBuilder.Entity<Kategoria>(entity =>
            {
                entity.HasIndex(e => e.Nev).IsUnique();
            });

            // Eszkoz konfiguráció
            modelBuilder.Entity<Eszkoz>(entity =>
            {
                entity.HasOne(e => e.Kategoria)
                      .WithMany(k => k.Eszkozok)
                      .HasForeignKey(e => e.KategoriaID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Foglalas konfiguráció
            modelBuilder.Entity<Foglalas>(entity =>
            {
                entity.HasOne(f => f.Eszkoz)
                      .WithMany(e => e.Foglalasok)
                      .HasForeignKey(f => f.EszkozID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed adatok
            SeedData(modelBuilder);
        }

        static void SeedData(ModelBuilder modelBuilder)
        {
            // Kategóriák
            modelBuilder.Entity<Kategoria>().HasData(
                new Kategoria
                {
                    KategoriaID = 1,
                    Nev = "kézi szerszámok",
                    KepUrl = "https://www.praktiker.hu/_next/image?url=https%3A%2F%2Fwebimg.praktiker.hu%2F_upload%2Fpraktiker_magazine_category_pic%2F81%2Fimage%2FGettyImages-546947363.jpg&w=600&q=75"
                },
                new Kategoria
                {
                    KategoriaID = 2,
                    Nev = "mérõmûszerek",
                    KepUrl = "https://www.centertool.hu/wp-content/uploads/2012/01/hosszmerok.jpg"
                },
                new Kategoria
                {
                    KategoriaID = 3,
                    Nev = "kerti eszközök",
                    KepUrl = "https://www.kertpont.hu/wp-content/uploads/2024/02/kerti-szerszamok.jpg"
                }
            );


            // Eszközök
            modelBuilder.Entity<Eszkoz>().HasData(
                new Eszkoz
                {
                    EszkozID = 1,
                    KategoriaID = 1,
                    Nev = "parkside x20 csavarbehajtó",
                    Leiras = "18V-os akkus csavarbehajtó, 2 akkuval",
                    Vetelar = 19990,
                    KiadasiAr = 1500,
                    BeszerzesiDatum = new DateTime(2024, 1, 15),
                    Status = EszkozStatus.Elerheto,
                    KepUrl = "https://www.lidl.ee/static/assets/Card-010-1920x1440px-1715742.jpg"
                },
                new Eszkoz
                {
                    EszkozID = 2,
                    KategoriaID = 1,
                    Nev = "einhell sarokcsiszoló",
                    Leiras = "230mm koronggal, 2200W teljesítmény",
                    Vetelar = 23000,
                    KiadasiAr = 1500,
                    BeszerzesiDatum = new DateTime(2024, 2, 20),
                    Status = EszkozStatus.Elerheto,
                    KepUrl = "https://www.aktiv.hu/img/70227/112531_altpic_1/500x500/112531.webp?time=1720778937"
                },
                new Eszkoz
                {
                    EszkozID = 3,
                    KategoriaID = 2,
                    Nev = "rétegvastagságmérõ",
                    Leiras = "Festékvastagság mérésére alkalmas digitális mûszer",
                    Vetelar = 43500,
                    KiadasiAr = 2000,
                    BeszerzesiDatum = new DateTime(2024, 3, 10),
                    Status = EszkozStatus.Elerheto,
                    KepUrl = "https://storagenahumain.blob.core.windows.net/strapi-media/0014_thumb_0aa1d5478d.jpg"
                },
                new Eszkoz
                {
                    EszkozID = 4,
                    KategoriaID = 2,
                    Nev = "infra hõmérõ",
                    Leiras = "Érintésmentes hõmérsékletmérõ -50 és 800°C között",
                    Vetelar = 87999,
                    KiadasiAr = 5000,
                    BeszerzesiDatum = new DateTime(2024, 4, 5),
                    Status = EszkozStatus.Elerheto,
                    KepUrl = "https://img-1.kwcdn.com/product/fancy/558dfa16-f249-45d5-ad38-3841b1577724.jpg?imageView2/2/w/800/q/70/format/avif"
                },
                new Eszkoz
                {
                    EszkozID = 5,
                    KategoriaID = 3,
                    Nev = "sörpad",
                    Leiras = "Összecsukható fa sörpad garnitúra",
                    Vetelar = 35000,
                    KiadasiAr = 500,
                    BeszerzesiDatum = new DateTime(2023, 5, 12),
                    Status = EszkozStatus.Elerheto,
                    KepUrl = "https://cdn.myshoptet.com/usr/www.bavord.hu/user/shop/big/6494-1_sorpad-garnitura-hattalaval.jpg?66504c5a"
                },
                new Eszkoz
                {
                    EszkozID = 6,
                    KategoriaID = 3,
                    Nev = "bográcsállvány",
                    Leiras = "Háromlábú bográcsállvány, állítható lánccal",
                    Vetelar = 17000,
                    KiadasiAr = 500,
                    BeszerzesiDatum = new DateTime(2023, 6, 1),
                    Status = EszkozStatus.Elerheto,
                    KepUrl = "https://xn--ednyek-cva.hu/storage/images/cache/data/Bogr%C3%A1cs%C3%A1llv%C3%A1nyok/IMG_3473-max1920-max1080.JPG"
                }
            );

            // Példa foglalások
            modelBuilder.Entity<Foglalas>().HasData(
                new Foglalas
                {
                    FoglalasID = 1,
                    EszkozID = 1,
                    Nev = "Kovács János",
                    Email = "kovacs.janos@example.com",
                    Telefonszam = "+36301234567",
                    Cim = "Budapest, Fõ utca 1.",
                    FoglalasKezdete = new DateTime(2025, 1, 10, 9, 0, 0),
                    FoglalasVege = new DateTime(2025, 1, 10, 19, 0, 0),
                    OraSzam = 10,
                    Bevetel = 15000,
                    Status = FoglalasStatus.Lezart,
                    LetrehozasDatum = new DateTime(2025, 1, 9, 14, 30, 0)
                },
                new Foglalas
                {
                    FoglalasID = 2,
                    EszkozID = 5,
                    Nev = "Nagy Péter",
                    Email = "nagy.peter@example.com",
                    Telefonszam = "+36209876543",
                    Cim = "Szeged, Kossuth utca 12.",
                    FoglalasKezdete = new DateTime(2025, 3, 20, 10, 0, 0),
                    FoglalasVege = new DateTime(2025, 3, 21, 22, 0, 0),
                    OraSzam = 36,
                    Bevetel = 18000,
                    Status = FoglalasStatus.Lezart,
                    LetrehozasDatum = new DateTime(2025, 3, 18, 11, 15, 0)
                }
            );
        }
    }
}