using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SzerszamKolcsonzo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFoglalasStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NapiDij",
                table: "Eszkozok",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 1,
                column: "NapiDij",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 2,
                column: "NapiDij",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 3,
                column: "NapiDij",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 4,
                column: "NapiDij",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 5,
                column: "NapiDij",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Eszkozok",
                keyColumn: "EszkozID",
                keyValue: 6,
                column: "NapiDij",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "Foglalasok",
                keyColumn: "FoglalasID",
                keyValue: 1,
                column: "Status",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Foglalasok",
                keyColumn: "FoglalasID",
                keyValue: 2,
                column: "Status",
                value: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NapiDij",
                table: "Eszkozok");

            migrationBuilder.UpdateData(
                table: "Foglalasok",
                keyColumn: "FoglalasID",
                keyValue: 1,
                column: "Status",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Foglalasok",
                keyColumn: "FoglalasID",
                keyValue: 2,
                column: "Status",
                value: 2);
        }
    }
}
