using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SzerszamKolcsonzo.Controllers;
using SzerszamKolcsonzo.Models;
using SzerszamKolcsonzo.Tests.Helpers;

namespace SzerszamKolcsonzo.Tests.Controllers
{
    public class EszkozokControllerTests
    {
        // ================================================================
        // VISSZAHOZ
        // ================================================================

        [Fact]
        public async Task VisszahozEszkoz_KiadottEszkoz_SikeresesVisszahozas()
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

            var foglalas = new Foglalas
            {
                Eszkoz = eszkoz,
                Nev = "Teszt Elek",
                Email = "teszt@teszt.hu",
                Telefonszam = "06301234567",
                Cim = "Teszt utca 1.",
                FoglalasKezdete = DateTime.UtcNow.AddHours(-3),
                KiadasIdopontja = DateTime.UtcNow.AddHours(-2),
                Status = FoglalasStatus.Kiadva,
                LetrehozasDatum = DateTime.UtcNow.AddHours(-3)
            };
            context.Foglalasok.Add(foglalas);
            await context.SaveChangesAsync();

            var controller = new EszkozokController(context);

            // Act
            var result = await controller.VisszahozEszkoz(eszkoz.EszkozID);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            eszkoz.Status.Should().Be(EszkozStatus.Elerheto);
            foglalas.Status.Should().Be(FoglalasStatus.Lezart);
            foglalas.VisszahozasIdopontja.Should().NotBeNull();
            foglalas.FizetendoOsszeg.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task VisszahozEszkoz_NemLetezoEszkoz_NotFound()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();
            var controller = new EszkozokController(context);

            // Act
            var result = await controller.VisszahozEszkoz(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task VisszahozEszkoz_NemKiadottEszkoz_BadRequest()
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
                Status = EszkozStatus.Elerheto // nem Kiadva!
            };
            context.Eszkozok.Add(eszkoz);
            await context.SaveChangesAsync();

            var controller = new EszkozokController(context);

            // Act
            var result = await controller.VisszahozEszkoz(eszkoz.EszkozID);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ================================================================
        // KIVON
        // ================================================================

        [Fact]
        public async Task KivonEszkoz_ElerhetoEszkoz_SikeresKivonas()
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
                Status = EszkozStatus.Elerheto
            };
            context.Eszkozok.Add(eszkoz);
            await context.SaveChangesAsync();

            var controller = new EszkozokController(context);
            var dto = new KivonasDto("Szervizben van");

            // Act
            var result = await controller.KivonEszkoz(eszkoz.EszkozID, dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            eszkoz.Status.Should().Be(EszkozStatus.Kivonva);
            eszkoz.Megjegyzes.Should().Be("Szervizben van");
        }

        [Fact]
        public async Task KivonEszkoz_UresMegjegyzes_BadRequest()
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
                Status = EszkozStatus.Elerheto
            };
            context.Eszkozok.Add(eszkoz);
            await context.SaveChangesAsync();

            var controller = new EszkozokController(context);
            var dto = new KivonasDto(""); // üres megjegyzés

            // Act
            var result = await controller.KivonEszkoz(eszkoz.EszkozID, dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ================================================================
        // ELERHETO
        // ================================================================

        [Fact]
        public async Task ElerhetovaTetelEszkoz_KivontEszkoz_SikeresVisszaallitas()
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
                Status = EszkozStatus.Kivonva,
                Megjegyzes = "Szervizben volt"
            };
            context.Eszkozok.Add(eszkoz);
            await context.SaveChangesAsync();

            var controller = new EszkozokController(context);

            // Act
            var result = await controller.ElerhetovaTetelEszkoz(eszkoz.EszkozID);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            eszkoz.Status.Should().Be(EszkozStatus.Elerheto);
            eszkoz.Megjegyzes.Should().BeNull();
        }
    }
}