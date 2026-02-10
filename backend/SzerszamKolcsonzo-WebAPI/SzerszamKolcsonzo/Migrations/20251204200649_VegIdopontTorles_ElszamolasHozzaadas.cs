using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzerszamKolcsonzo.Migrations
{
    /// <inheritdoc />
    public partial class VegIdopontTorles_ElszamolasHozzaadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FoglalasVege",
                table: "Foglalasok");

            migrationBuilder.DropColumn(
                name: "OraSzam",
                table: "Foglalasok");

            migrationBuilder.AlterColumn<decimal>(
                name: "Bevetel",
                table: "Foglalasok",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ElszamolhatoIdo",
                table: "Foglalasok",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FizetendoOsszeg",
                table: "Foglalasok",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VisszahozasIdopontja",
                table: "Foglalasok",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 3,
                columns: new[] { "Leiras", "Nev" },
                values: new object[] { "Festékvastagság mérésére alkalmas digitális mûszer", "rétegvastagságmérõ" });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 4,
                columns: new[] { "Leiras", "Nev" },
                values: new object[] { "Érintésmentes hõmérsékletmérõ -50 és 800°C között", "infra hõmérõ" });

            migrationBuilder.UpdateData(
                table: "Foglalasok",
                keyColumn: "FoglalasID",
                keyValue: 1,
                columns: new[] { "Bevetel", "ElszamolhatoIdo", "FizetendoOsszeg", "KiadasIdopontja", "VisszahozasIdopontja" },
                values: new object[] { 14875m, 595, 14875m, new DateTime(2025, 1, 10, 9, 5, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 10, 19, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Foglalasok",
                keyColumn: "FoglalasID",
                keyValue: 2,
                columns: new[] { "Bevetel", "ElszamolhatoIdo", "FizetendoOsszeg", "KiadasIdopontja", "VisszahozasIdopontja" },
                values: new object[] { 17916.67m, 2150, 17916.67m, new DateTime(2025, 3, 20, 10, 10, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 21, 22, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Kategoriak",
                keyColumn: "KategoriaID",
                keyValue: 2,
                column: "Nev",
                value: "mérõmûszerek");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElszamolhatoIdo",
                table: "Foglalasok");

            migrationBuilder.DropColumn(
                name: "FizetendoOsszeg",
                table: "Foglalasok");

            migrationBuilder.DropColumn(
                name: "VisszahozasIdopontja",
                table: "Foglalasok");

            migrationBuilder.AlterColumn<int>(
                name: "Bevetel",
                table: "Foglalasok",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FoglalasVege",
                table: "Foglalasok",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "OraSzam",
                table: "Foglalasok",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 3,
                columns: new[] { "Leiras", "Nev" },
                values: new object[] { "Festékvastagság mérésére alkalmas digitális műszer", "rétegvastagságmérő" });

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 4,
                columns: new[] { "Leiras", "Nev" },
                values: new object[] { "Érintésmentes hőmérsékletmérő -50 és 800°C között", "infra hőmérő" });

            migrationBuilder.UpdateData(
                table: "Foglalasok",
                keyColumn: "FoglalasID",
                keyValue: 1,
                columns: new[] { "Bevetel", "FoglalasVege", "KiadasIdopontja", "OraSzam" },
                values: new object[] { 15000, new DateTime(2025, 1, 10, 19, 0, 0, 0, DateTimeKind.Unspecified), null, 10 });

            migrationBuilder.UpdateData(
                table: "Foglalasok",
                keyColumn: "FoglalasID",
                keyValue: 2,
                columns: new[] { "Bevetel", "FoglalasVege", "KiadasIdopontja", "OraSzam" },
                values: new object[] { 18000, new DateTime(2025, 3, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), null, 36 });

            migrationBuilder.UpdateData(
                table: "Kategoriak",
                keyColumn: "KategoriaID",
                keyValue: 2,
                column: "Nev",
                value: "mérőműszerek");
        }
    }
}
