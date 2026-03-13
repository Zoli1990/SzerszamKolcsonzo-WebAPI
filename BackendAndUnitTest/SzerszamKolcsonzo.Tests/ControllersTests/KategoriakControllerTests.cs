using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SzerszamKolcsonzo.Controllers;
using SzerszamKolcsonzo.DTOs;
using SzerszamKolcsonzo.Models;
using SzerszamKolcsonzo.Tests.Helpers;

namespace SzerszamKolcsonzo.Tests.Controllers
{
    public class KategoriakControllerTests
    {
        // ================================================================
        // GET
        // ================================================================

        [Fact]
        public async Task GetKategoriak_SeedAdatokkal_VissszaadjaOsszetKategoriat()
        {
            // Arrange
            var context = TestDbContextFactory.Create(); // seed adatokkal
            var controller = new KategoriakController(context);

            // Act
            var result = await controller.GetKategoriak();

            // Assert
            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var kategoriak = ok.Value.Should().BeAssignableTo<IEnumerable<KategoriaDto>>().Subject;
            kategoriak.Should().HaveCount(3); // 3 seed kategória
        }

        [Fact]
        public async Task GetKategoria_LetezoId_VisszaadjaKategoriat()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var controller = new KategoriakController(context);

            // Act
            var result = await controller.GetKategoria(1);

            // Assert
            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var kategoria = ok.Value.Should().BeOfType<KategoriaDto>().Subject;
            kategoria.KategoriaID.Should().Be(1);
            kategoria.Nev.Should().Be("kézi szerszámok");
        }

        [Fact]
        public async Task GetKategoria_NemLetezoId_NotFound()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var controller = new KategoriakController(context);

            // Act
            var result = await controller.GetKategoria(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        // ================================================================
        // CREATE
        // ================================================================

        [Fact]
        public async Task CreateKategoria_UjNev_SikeresLetrehozas()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();
            var controller = new KategoriakController(context);
            var dto = new CreateKategoriaDto("elektromos szerszámok", null);

            // Act
            var result = await controller.CreateKategoria(dto);

            // Assert
            var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var kategoria = created.Value.Should().BeOfType<KategoriaDto>().Subject;
            kategoria.Nev.Should().Be("elektromos szerszámok");
            context.Kategoriak.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateKategoria_DuplikaltNev_BadRequest()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();
            context.Kategoriak.Add(new Kategoria { Nev = "elektromos szerszámok" });
            await context.SaveChangesAsync();

            var controller = new KategoriakController(context);
            var dto = new CreateKategoriaDto("elektromos szerszámok", null); // ugyanaz a név!

            // Act
            var result = await controller.CreateKategoria(dto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ================================================================
        // UPDATE
        // ================================================================

        [Fact]
        public async Task UpdateKategoria_LetezoKategoria_SikeresModositas()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();
            var kategoria = new Kategoria { Nev = "régi név", KepUrl = null };
            context.Kategoriak.Add(kategoria);
            await context.SaveChangesAsync();

            var controller = new KategoriakController(context);
            var dto = new UpdateKategoriaDto("új név", "https://pelda.hu/kep.jpg");

            // Act
            var result = await controller.UpdateKategoria(kategoria.KategoriaID, dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            kategoria.Nev.Should().Be("új név");
            kategoria.KepUrl.Should().Be("https://pelda.hu/kep.jpg");
        }

        [Fact]
        public async Task UpdateKategoria_NemLetezoKategoria_NotFound()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();
            var controller = new KategoriakController(context);
            var dto = new UpdateKategoriaDto("új név", null);

            // Act
            var result = await controller.UpdateKategoria(999, dto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateKategoria_DuplikaltNev_BadRequest()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();
            context.Kategoriak.Add(new Kategoria { Nev = "foglalt név" });
            var kategoria = new Kategoria { Nev = "saját név" };
            context.Kategoriak.Add(kategoria);
            await context.SaveChangesAsync();

            var controller = new KategoriakController(context);
            var dto = new UpdateKategoriaDto("foglalt név", null); // már létező név!

            // Act
            var result = await controller.UpdateKategoria(kategoria.KategoriaID, dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // ================================================================
        // DELETE
        // ================================================================

        [Fact]
        public async Task DeleteKategoria_EszkozNelkuli_SikeresTorles()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();
            var kategoria = new Kategoria { Nev = "törölhető kategória" };
            context.Kategoriak.Add(kategoria);
            await context.SaveChangesAsync();

            var controller = new KategoriakController(context);

            // Act
            var result = await controller.DeleteKategoria(kategoria.KategoriaID);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            context.Kategoriak.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteKategoria_EszkozokkelRendelkezo_BadRequest()
        {
            // Arrange
            var context = TestDbContextFactory.CreateEmpty();
            var kategoria = new Kategoria { Nev = "nem törölhető" };
            context.Kategoriak.Add(kategoria);
            await context.SaveChangesAsync();

            context.Eszkozok.Add(new Eszkoz
            {
                KategoriaID = kategoria.KategoriaID,
                Nev = "Teszt eszköz",
                Vetelar = 10000,
                KiadasiAr = 1000,
                BeszerzesiDatum = DateTime.UtcNow,
                Status = EszkozStatus.Elerheto
            });
            await context.SaveChangesAsync();

            var controller = new KategoriakController(context);

            // Act
            var result = await controller.DeleteKategoria(kategoria.KategoriaID);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}