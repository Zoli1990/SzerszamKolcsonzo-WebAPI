using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SzerszamKolcsonzo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Kategoriak",
                columns: table => new
                {
                    KategoriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nev = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriak", x => x.KategoriaID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Eszkozok",
                columns: table => new
                {
                    EszkozID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    KategoriaID = table.Column<int>(type: "int", nullable: false),
                    Nev = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Leiras = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Vetelar = table.Column<int>(type: "int", nullable: false),
                    KiadasiAr = table.Column<int>(type: "int", nullable: false),
                    BeszerzesiDatum = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eszkozok", x => x.EszkozID);
                    table.ForeignKey(
                        name: "FK_Eszkozok_Kategoriak_KategoriaID",
                        column: x => x.KategoriaID,
                        principalTable: "Kategoriak",
                        principalColumn: "KategoriaID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Foglalasok",
                columns: table => new
                {
                    FoglalasID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EszkozID = table.Column<int>(type: "int", nullable: false),
                    Nev = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefonszam = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cim = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FoglalasKezdete = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FoglalasVege = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OraSzam = table.Column<int>(type: "int", nullable: false),
                    Bevetel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LetrehozasDatum = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foglalasok", x => x.FoglalasID);
                    table.ForeignKey(
                        name: "FK_Foglalasok_Eszkozok_EszkozID",
                        column: x => x.EszkozID,
                        principalTable: "Eszkozok",
                        principalColumn: "EszkozID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Kategoriak",
                columns: new[] { "KategoriaID", "Nev" },
                values: new object[,]
                {
                    { 1, "kézi szerszámok" },
                    { 2, "mérőműszerek" },
                    { 3, "kerti eszközök" }
                });

            migrationBuilder.InsertData(
                table: "Eszkozok",
                columns: new[] { "EszkozID", "BeszerzesiDatum", "KategoriaID", "KiadasiAr", "Leiras", "Nev", "Status", "Vetelar" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1500, "18V-os akkus csavarbehajtó, 2 akkuval", "parkside x20 csavarbehajtó", 0, 19990 },
                    { 2, new DateTime(2024, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1500, "230mm koronggal, 2200W teljesítmény", "einhell sarokcsiszoló", 0, 23000 },
                    { 3, new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 2000, "Festékvastagság mérésére alkalmas digitális műszer", "rétegvastagságmérő", 0, 43500 },
                    { 4, new DateTime(2024, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 5000, "Érintésmentes hőmérsékletmérő -50 és 800°C között", "infra hőmérő", 0, 87999 },
                    { 5, new DateTime(2023, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 500, "Összecsukható fa sörpad garnitúra", "sörpad", 0, 35000 },
                    { 6, new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 500, "Háromlábú bográcsállvány, állítható lánccal", "bográcsállvány", 0, 17000 }
                });

            migrationBuilder.InsertData(
                table: "Foglalasok",
                columns: new[] { "FoglalasID", "Bevetel", "Cim", "Email", "EszkozID", "FoglalasKezdete", "FoglalasVege", "LetrehozasDatum", "Nev", "OraSzam", "Status", "Telefonszam" },
                values: new object[,]
                {
                    { 1, 15000, "Budapest, Fő utca 1.", "kovacs.janos@example.com", 1, new DateTime(2025, 1, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 10, 19, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 9, 14, 30, 0, 0, DateTimeKind.Unspecified), "Kovács János", 10, 1, "+36301234567" },
                    { 2, 18000, "Szeged, Kossuth utca 12.", "nagy.peter@example.com", 5, new DateTime(2025, 3, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 21, 22, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 18, 11, 15, 0, 0, DateTimeKind.Unspecified), "Nagy Péter", 36, 1, "+36209876543" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Eszkozok_KategoriaID",
                table: "Eszkozok",
                column: "KategoriaID");

            migrationBuilder.CreateIndex(
                name: "IX_Foglalasok_EszkozID",
                table: "Foglalasok",
                column: "EszkozID");

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriak_Nev",
                table: "Kategoriak",
                column: "Nev",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foglalasok");

            migrationBuilder.DropTable(
                name: "Eszkozok");

            migrationBuilder.DropTable(
                name: "Kategoriak");
        }
    }
}
