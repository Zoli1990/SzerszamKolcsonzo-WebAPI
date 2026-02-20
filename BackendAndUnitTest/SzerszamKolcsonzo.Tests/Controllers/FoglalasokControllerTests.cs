using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SzerszamKolcsonzo.Controllers;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Features.Push.Services;
using SzerszamKolcsonzo.Models;
using SzerszamKolcsonzo.Tests.Helpers;

namespace SzerszamKolcsonzo.Tests.Controllers
{
    public class FoglalasokControllerTests
    {
        private readonly Mock<ILogger<FoglalasokController>> _loggerMock;

        public FoglalasokControllerTests()
        {
            _loggerMock = new Mock<ILogger<FoglalasokController>>();
        }

        private FoglalasokController CreateController(AppDbContext context)
        {
            // PushNotificationService helyett null-t adunk át
            // a tesztelt metódusok (Kiadas, Lezaras, Torles) nem használják
            return new FoglalasokController(context, _loggerMock.Object, null!);
        }

        //MOCKOLT (nem fut jóra) 7bd sikertelen teszt
        //public class FoglalasokControllerTests
        //{
        //    private readonly Mock<ILogger<FoglalasokController>> _loggerMock;
        //    private readonly Mock<PushNotificationService> _pushServiceMock;

        //    public FoglalasokControllerTests()
        //    {
        //        _loggerMock = new Mock<ILogger<FoglalasokController>>();
        //        _pushServiceMock = new Mock<PushNotificationService>();
        //    }

        //    private FoglalasokController CreateController(AppDbContext context)
        //    {
        //        return new FoglalasokController(context, _loggerMock.Object, _pushServiceMock.Object);
        //    }

        // ================================================================
        // KIADAS
        // ================================================================

        [Fact]
        public async Task Kiadas_FoglaltFoglalas_SikeresKiadas()
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

            var foglalas = new Foglalas
            {
                Eszkoz = eszkoz,
                Nev = "Teszt Elek",
                Email = "teszt@teszt.hu",
                Telefonszam = "06301234567",
                Cim = "Teszt utca 1.",
                FoglalasKezdete = DateTime.UtcNow,
                Status = FoglalasStatus.Foglalva,
                LetrehozasDatum = DateTime.UtcNow
            };
            context.Foglalasok.Add(foglalas);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.Kiadas(foglalas.FoglalasID);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            foglalas.Status.Should().Be(FoglalasStatus.Kiadva);
            foglalas.KiadasIdopontja.Should().NotBeNull();
            eszkoz.Status.Should().Be(EszkozStatus.Kiadva);
        }

        [Fact]
        public async Task Kiadas_NemLetezoFoglalas_NotFound()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();
            var controller = CreateController(context);

            // Act
            var result = await controller.Kiadas(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Kiadas_MarKiadottFoglalas_BadRequest()
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
                FoglalasKezdete = DateTime.UtcNow,
                Status = FoglalasStatus.Kiadva, // már ki van adva!
                LetrehozasDatum = DateTime.UtcNow
            };
            context.Foglalasok.Add(foglalas);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.Kiadas(foglalas.FoglalasID);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ================================================================
        // LEZARAS
        // ================================================================

        [Fact]
        public async Task Lezaras_KiadottFoglalas_SikeresLezaras()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();

            var eszkoz = new Eszkoz
            {
                KategoriaID = 1,
                Nev = "Teszt fúró",
                Vetelar = 10000,
                KiadasiAr = 1200, // 1200 Ft/óra = 20 Ft/perc
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
                KiadasIdopontja = DateTime.UtcNow.AddHours(-2), // 2 órája ki van adva
                Status = FoglalasStatus.Kiadva,
                LetrehozasDatum = DateTime.UtcNow.AddHours(-3)
            };
            context.Foglalasok.Add(foglalas);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.Lezaras(foglalas.FoglalasID);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            foglalas.Status.Should().Be(FoglalasStatus.Lezart);
            foglalas.VisszahozasIdopontja.Should().NotBeNull();
            foglalas.ElszamolhatoIdo.Should().BeGreaterThan(0);
            foglalas.FizetendoOsszeg.Should().BeGreaterThan(0);
            eszkoz.Status.Should().Be(EszkozStatus.Elerheto);
        }

        [Fact]
        public async Task Lezaras_NemKiadottFoglalas_BadRequest()
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

            var foglalas = new Foglalas
            {
                Eszkoz = eszkoz,
                Nev = "Teszt Elek",
                Email = "teszt@teszt.hu",
                Telefonszam = "06301234567",
                Cim = "Teszt utca 1.",
                FoglalasKezdete = DateTime.UtcNow,
                Status = FoglalasStatus.Foglalva, // még nincs kiadva!
                LetrehozasDatum = DateTime.UtcNow
            };
            context.Foglalasok.Add(foglalas);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.Lezaras(foglalas.FoglalasID);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ================================================================
        // TORLES
        // ================================================================

        [Fact]
        public async Task Torles_FoglaltFoglalas_SikeresTorles()
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

            var foglalas = new Foglalas
            {
                Eszkoz = eszkoz,
                Nev = "Teszt Elek",
                Email = "teszt@teszt.hu",
                Telefonszam = "06301234567",
                Cim = "Teszt utca 1.",
                FoglalasKezdete = DateTime.UtcNow,
                Status = FoglalasStatus.Foglalva,
                LetrehozasDatum = DateTime.UtcNow
            };
            context.Foglalasok.Add(foglalas);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.Torles(foglalas.FoglalasID);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            foglalas.Status.Should().Be(FoglalasStatus.Torolt);
            eszkoz.Status.Should().Be(EszkozStatus.Elerheto);
        }

        [Fact]
        public async Task Torles_LezartFoglalas_BadRequest()
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

            var foglalas = new Foglalas
            {
                Eszkoz = eszkoz,
                Nev = "Teszt Elek",
                Email = "teszt@teszt.hu",
                Telefonszam = "06301234567",
                Cim = "Teszt utca 1.",
                FoglalasKezdete = DateTime.UtcNow,
                Status = FoglalasStatus.Lezart, // már lezárt!
                LetrehozasDatum = DateTime.UtcNow
            };
            context.Foglalasok.Add(foglalas);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.Torles(foglalas.FoglalasID);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}