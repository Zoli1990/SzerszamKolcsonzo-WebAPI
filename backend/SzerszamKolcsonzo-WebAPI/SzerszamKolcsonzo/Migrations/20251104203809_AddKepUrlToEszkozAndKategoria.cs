using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzerszamKolcsonzo.Migrations
{
    /// <inheritdoc />
    public partial class AddKepUrlToEszkozAndKategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KepUrl",
                table: "Kategoriak",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "KepUrl",
                table: "Eszkozok",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 1,
                column: "KepUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 2,
                column: "KepUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 3,
                column: "KepUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 4,
                column: "KepUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 5,
                column: "KepUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 6,
                column: "KepUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Kategoriak",
                keyColumn: "KategoriaID",
                keyValue: 1,
                column: "KepUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Kategoriak",
                keyColumn: "KategoriaID",
                keyValue: 2,
                column: "KepUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Kategoriak",
                keyColumn: "KategoriaID",
                keyValue: 3,
                column: "KepUrl",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KepUrl",
                table: "Kategoriak");

            migrationBuilder.DropColumn(
                name: "KepUrl",
                table: "Eszkozok");
        }
    }
}
