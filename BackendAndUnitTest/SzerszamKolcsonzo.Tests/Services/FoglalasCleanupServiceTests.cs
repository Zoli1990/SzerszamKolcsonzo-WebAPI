using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Models;
using SzerszamKolcsonzo.Services;
using SzerszamKolcsonzo.Tests.Helpers;

namespace SzerszamKolcsonzo.Tests.Services
{
    public class FoglalasCleanupServiceTests
    {
        private IServiceProvider CreateServiceProvider(AppDbContext context)
        {
            var services = new ServiceCollection();
            services.AddSingleton(context);
            services.AddSingleton<AppDbContext>(context);

            // EF context factory regisztrálása
            services.AddScoped(_ => context);

            return services.BuildServiceProvider();
        }

        private FoglalasCleanupService CreateService(IServiceProvider serviceProvider)
        {
            var logger = new Mock<ILogger<FoglalasCleanupService>>().Object;
            return new FoglalasCleanupService(serviceProvider, logger);
        }

        // ================================================================
        // LEJÁRT FOGLALÁSOK TÖRLÉSE
        // ================================================================

        [Fact]
        public async Task Cleanup_LejartFoglalas_AutomatikusanTorolve()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();

            var eszkoz = new Eszkoz
            {
                KategoriaID = 1,
                Nev = "Teszt fúró",
                Vetelar = 10000,
                KiadasiAr = 1000,
                BeszerzesiDatum = DateTime.UtcNow,
                Status = EszkozStatus.Foglalva
            };
            context.Eszkozok.Add(eszkoz);

            var lejartFoglalas = new Foglalas
            {
                Eszkoz = eszkoz,
                Nev = "Teszt Elek",
                Email = "teszt@teszt.hu",
                Telefonszam = "06301234567",
                Cim = "Teszt utca 1.",
                FoglalasKezdete = DateTime.UtcNow.AddMinutes(-30),
                Status = FoglalasStatus.Foglalva,
                LetrehozasDatum = DateTime.UtcNow.AddMinutes(-20), // 20 perce létrehozva → lejárt
                KiadasIdopontja = null
            };
            context.Foglalasok.Add(lejartFoglalas);
            await context.SaveChangesAsync();

            var serviceProvider = CreateServiceProvider(context);
            var service = CreateService(serviceProvider);

            // Act - privát metódust CancellationToken-nel hívjuk
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            // A service ExecuteAsync-t hív, de mi közvetlenül
            // a cleanup logikát teszteljük reflection segítségével
            var method = typeof(FoglalasCleanupService)
                .GetMethod("TorolLejartFoglalasokat",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

            await (Task)method!.Invoke(service, null)!;

            // Assert
            lejartFoglalas.Status.Should().Be(FoglalasStatus.Torolt);
            eszkoz.Status.Should().Be(EszkozStatus.Elerheto);
        }

        [Fact]
        public async Task Cleanup_MegNemLejartFoglalas_NemTorolve()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();

            var eszkoz = new Eszkoz
            {
                KategoriaID = 1,
                Nev = "Teszt fúró",
                Vetelar = 10000,
                KiadasiAr = 1000,
                BeszerzesiDatum = DateTime.UtcNow,
                Status = EszkozStatus.Foglalva
            };
            context.Eszkozok.Add(eszkoz);

            var frissFoglalas = new Foglalas
            {
                Eszkoz = eszkoz,
                Nev = "Teszt Elek",
                Email = "teszt@teszt.hu",
                Telefonszam = "06301234567",
                Cim = "Teszt utca 1.",
                FoglalasKezdete = DateTime.UtcNow,
                Status = FoglalasStatus.Foglalva,
                LetrehozasDatum = DateTime.UtcNow.AddMinutes(-5), // csak 5 perce → még nem járt le
                KiadasIdopontja = null
            };
            context.Foglalasok.Add(frissFoglalas);
            await context.SaveChangesAsync();

            var serviceProvider = CreateServiceProvider(context);
            var service = CreateService(serviceProvider);

            // Act
            var method = typeof(FoglalasCleanupService)
                .GetMethod("TorolLejartFoglalasokat",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

            await (Task)method!.Invoke(service, null)!;

            // Assert
            frissFoglalas.Status.Should().Be(FoglalasStatus.Foglalva); // nem változott
            eszkoz.Status.Should().Be(EszkozStatus.Foglalva); // nem szabadult fel
        }

        [Fact]
        public async Task Cleanup_KiadottFoglalas_NemTorolve()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();

            var eszkoz = new Eszkoz
            {
                KategoriaID = 1,
                Nev = "Teszt fúró",
                Vetelar = 10000,
                KiadasiAr = 1000,
                BeszerzesiDatum = DateTime.UtcNow,
                Status = EszkozStatus.Kiadva
            };
            context.Eszkozok.Add(eszkoz);

            var kiadottFoglalas = new Foglalas
            {
                Eszkoz = eszkoz,
                Nev = "Teszt Elek",
                Email = "teszt@teszt.hu",
                Telefonszam = "06301234567",
                Cim = "Teszt utca 1.",
                FoglalasKezdete = DateTime.UtcNow.AddHours(-3),
                Status = FoglalasStatus.Kiadva, // már ki van adva → nem törölhető
                LetrehozasDatum = DateTime.UtcNow.AddHours(-3),
                KiadasIdopontja = DateTime.UtcNow.AddHours(-2)
            };
            context.Foglalasok.Add(kiadottFoglalas);
            await context.SaveChangesAsync();

            var serviceProvider = CreateServiceProvider(context);
            var service = CreateService(serviceProvider);

            // Act
            var method = typeof(FoglalasCleanupService)
                .GetMethod("TorolLejartFoglalasokat",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

            await (Task)method!.Invoke(service, null)!;

            // Assert
            kiadottFoglalas.Status.Should().Be(FoglalasStatus.Kiadva); // nem változott
            eszkoz.Status.Should().Be(EszkozStatus.Kiadva); // nem változott
        }

        [Fact]
        public async Task Cleanup_TobBLejartFoglalas_MindenTorolve()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();

            var eszkoz1 = new Eszkoz
            {
                KategoriaID = 1,
                Nev = "Fúró",
                Vetelar = 10000,
                KiadasiAr = 1000,
                BeszerzesiDatum = DateTime.UtcNow,
                Status = EszkozStatus.Foglalva
            };
            var eszkoz2 = new Eszkoz
            {
                KategoriaID = 1,
                Nev = "Csiszoló",
                Vetelar = 20000,
                KiadasiAr = 2000,
                BeszerzesiDatum = DateTime.UtcNow,
                Status = EszkozStatus.Foglalva
            };
            context.Eszkozok.AddRange(eszkoz1, eszkoz2);

            var foglalas1 = new Foglalas
            {
                Eszkoz = eszkoz1,
                Nev = "Teszt Elek",
                Email = "teszt@teszt.hu",
                Telefonszam = "06301234567",
                Cim = "Teszt utca 1.",
                FoglalasKezdete = DateTime.UtcNow.AddMinutes(-30),
                Status = FoglalasStatus.Foglalva,
                LetrehozasDatum = DateTime.UtcNow.AddMinutes(-20),
                KiadasIdopontja = null
            };
            var foglalas2 = new Foglalas
            {
                Eszkoz = eszkoz2,
                Nev = "Másik Elek",
                Email = "masik@teszt.hu",
                Telefonszam = "06309999999",
                Cim = "Másik utca 2.",
                FoglalasKezdete = DateTime.UtcNow.AddMinutes(-60),
                Status = FoglalasStatus.Foglalva,
                LetrehozasDatum = DateTime.UtcNow.AddMinutes(-45),
                KiadasIdopontja = null
            };
            context.Foglalasok.AddRange(foglalas1, foglalas2);
            await context.SaveChangesAsync();

            var serviceProvider = CreateServiceProvider(context);
            var service = CreateService(serviceProvider);

            // Act
            var method = typeof(FoglalasCleanupService)
                .GetMethod("TorolLejartFoglalasokat",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

            await (Task)method!.Invoke(service, null)!;

            // Assert
            foglalas1.Status.Should().Be(FoglalasStatus.Torolt);
            foglalas2.Status.Should().Be(FoglalasStatus.Torolt);
            eszkoz1.Status.Should().Be(EszkozStatus.Elerheto);
            eszkoz2.Status.Should().Be(EszkozStatus.Elerheto);
        }
    }
}